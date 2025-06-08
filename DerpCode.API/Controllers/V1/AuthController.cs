using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Requests.Auth;
using DerpCode.API.Models.Responses.Auth;
using DerpCode.API.Models.Settings;
using DerpCode.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using static DerpCode.API.Utilities.UtilityFunctions;

namespace DerpCode.API.Controllers.V1;

[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1")]
[ApiController]
public sealed class AuthController : ServiceControllerBase
{
    private readonly IUserRepository userRepository;

    private readonly IJwtTokenService jwtTokenService;

    private readonly IEmailService emailService;

    private readonly IEmailTemplateService emailTemplateService;

    private readonly IGitHubOAuthService gitHubOAuthService;

    private readonly IGoogleOAuthService googleOAuthService;

    private readonly AuthenticationSettings authSettings;

    public AuthController(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        IGitHubOAuthService gitHubOAuthService,
        IGoogleOAuthService googleOAuthService,
        IOptions<AuthenticationSettings> authSettings,
        ICorrelationIdService correlationIdService)
            : base(correlationIdService)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        this.emailTemplateService = emailTemplateService ?? throw new ArgumentNullException(nameof(emailTemplateService));
        this.gitHubOAuthService = gitHubOAuthService ?? throw new ArgumentNullException(nameof(gitHubOAuthService));
        this.googleOAuthService = googleOAuthService ?? throw new ArgumentNullException(nameof(googleOAuthService));
        this.authSettings = authSettings?.Value ?? throw new ArgumentNullException(nameof(authSettings));
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="registerUserRequest">The register request object.</param>
    /// <returns>The user object and tokens.</returns>
    /// <response code="201">The user object and tokens.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [AllowAnonymous]
    [HttpPost("register", Name = nameof(RegisterAsync))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<LoginResponse>> RegisterAsync([FromBody] RegisterUserRequest registerUserRequest)
    {
        if (registerUserRequest == null)
        {
            return this.BadRequest();
        }

        var user = registerUserRequest.ToEntity();

        var result = await this.userRepository.CreateUserWithPasswordAsync(user, registerUserRequest.Password, this.HttpContext.RequestAborted);

        if (!result.Succeeded)
        {
            return this.BadRequest([.. result.Errors.Select(e => e.Description)]);
        }

        var token = await this.GenerateAndSaveAccessAndRefreshTokensAsync(user, registerUserRequest.DeviceId);

        await this.SendConfirmEmailLink(user);

        var userToReturn = UserDto.FromEntity(user);

        return this.CreatedAtRoute(
            nameof(UsersController.GetUserAsync),
            new
            {
                controller = GetControllerName<UsersController>(),
                id = user.Id
            },
            new LoginResponse
            {
                Token = token,
                User = userToReturn
            });
    }

    /// <summary>
    /// Logs the user in.
    /// </summary>
    /// <param name="loginRequest">The login request object.</param>
    /// <returns>The user object and tokens.</returns>
    /// <response code="200">The user object and tokens.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If provided login information is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [AllowAnonymous]
    [HttpPost("login", Name = nameof(LoginAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null)
        {
            return this.BadRequest();
        }

        var user = await this.userRepository.GetByUsernameAsync(loginRequest.Username, [user => user.RefreshTokens], this.HttpContext.RequestAborted)
            ?? await this.userRepository.GetByEmailAsync(loginRequest.Username, [user => user.RefreshTokens], this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.Unauthorized("Invalid username or password.");
        }

        var result = await this.userRepository.CheckPasswordAsync(user, loginRequest.Password, this.HttpContext.RequestAborted);

        if (!result)
        {
            return this.Unauthorized("Invalid username or password.");
        }

        var token = await this.GenerateAndSaveAccessAndRefreshTokensAsync(user, loginRequest.DeviceId);

        var userToReturn = UserDto.FromEntity(user);

        return this.Ok(
            new LoginResponse
            {
                Token = token,
                User = userToReturn
            });
    }

    /// <summary>
    /// Logs the user in using Google OAuth authorization code (similar to GitHub flow).
    /// </summary>
    /// <param name="loginRequest">The login request object.</param>
    /// <returns>The user object and tokens.</returns>
    /// <response code="200">If the user was logged in.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If provided login information is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [AllowAnonymous]
    [HttpPost("login/google", Name = nameof(LoginGoogleAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> LoginGoogleAsync([FromBody] OAuthCodeLoginRequest loginRequest)
    {
        try
        {
            if (loginRequest == null)
            {
                return this.BadRequest("Request body cannot be null.");
            }

            var idToken = await this.googleOAuthService.ExchangeCodeForGoogleIdTokenAsync(loginRequest.Code, this.HttpContext.RequestAborted);

            if (string.IsNullOrWhiteSpace(idToken))
            {
                return this.Unauthorized("Invalid Google authorization code.");
            }

            var validatedToken = await this.googleOAuthService.ValidateIdTokenAsync(idToken, this.HttpContext.RequestAborted);

            var user = await this.userRepository.GetByLinkedAccountAsync(validatedToken.Subject, LinkedAccountType.Google, [user => user.RefreshTokens], this.HttpContext.RequestAborted);

            if (user == null)
            {
                // Try to find user by email if no linked account exists
                if (!string.IsNullOrWhiteSpace(validatedToken.Email))
                {
                    user = await this.userRepository.GetByEmailAsync(validatedToken.Email, [user => user.RefreshTokens], this.HttpContext.RequestAborted);
                }

                if (user != null)
                {
                    // Link the Google account to the existing user
                    user.LinkedAccounts.Add(new LinkedAccount
                    {
                        Id = validatedToken.Subject,
                        LinkedAccountType = LinkedAccountType.Google
                    });

                    user.EmailConfirmed = user.EmailConfirmed || validatedToken.EmailVerified;

                    var updated = await this.userRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

                    if (updated == 0)
                    {
                        return this.InternalServerError("Unable to link Google account to existing user.");
                    }
                }
                else
                {
                    // Create a new user with Google account
                    var newUser = new User
                    {
                        UserName = validatedToken.Email?.Split('@').FirstOrDefault() ?? validatedToken.Subject,
                        Email = validatedToken.Email,
                        EmailConfirmed = validatedToken.EmailVerified,
                        LinkedAccounts =
                        [
                            new LinkedAccount
                            {
                                Id = validatedToken.Subject,
                                LinkedAccountType = LinkedAccountType.Google
                            }
                        ]
                    };

                    var createResult = await this.userRepository.CreateUserWithoutPasswordAsync(newUser, this.HttpContext.RequestAborted);

                    if (!createResult.Succeeded)
                    {
                        return this.BadRequest([.. createResult.Errors.Select(e => e.Description)]);
                    }

                    user = newUser;

                    // Optionally send confirmation email if email is not confirmed
                    if (!newUser.EmailConfirmed && !string.IsNullOrWhiteSpace(newUser.Email))
                    {
                        await this.SendConfirmEmailLink(newUser);
                    }
                }
            }

            var token = await this.GenerateAndSaveAccessAndRefreshTokensAsync(user, loginRequest.DeviceId);
            var userToReturn = UserDto.FromEntity(user);

            return this.Ok(
                new LoginResponse
                {
                    Token = token,
                    User = userToReturn
                });
        }
        catch (HttpRequestException)
        {
            return this.Unauthorized("Unable to validate Google authorization code.");
        }
        catch
        {
            return this.InternalServerError("Unable to login with Google.");
        }
    }

    /// <summary>
    /// Logs the user in using GitHub OAuth credentials.
    /// </summary>
    /// <param name="loginRequest">The login request object.</param>
    /// <returns>The user object and tokens.</returns>
    /// <response code="200">If the user was logged in.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If provided login information is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [AllowAnonymous]
    [HttpPost("login/github", Name = nameof(LoginGitHubAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> LoginGitHubAsync([FromBody] OAuthCodeLoginRequest loginRequest)
    {
        try
        {
            if (loginRequest == null)
            {
                return this.BadRequest("Request body cannot be null.");
            }

            var githubToken = await this.gitHubOAuthService.ExchangeCodeForGithubAccessTokenAsync(loginRequest.Code, this.HttpContext.RequestAborted);

            if (string.IsNullOrWhiteSpace(githubToken))
            {
                return this.Unauthorized("Invalid GitHub code.");
            }

            var gitHubUserInfo = await this.gitHubOAuthService.GetGitHubUser(githubToken, this.HttpContext.RequestAborted);

            if (gitHubUserInfo == null || string.IsNullOrWhiteSpace(gitHubUserInfo.Login))
            {
                return this.Unauthorized("Unable to retrieve GitHub user information.");
            }

            var user = await this.userRepository.GetByLinkedAccountAsync(gitHubUserInfo.Id.ToString(CultureInfo.InvariantCulture), LinkedAccountType.GitHub, [user => user.RefreshTokens], this.HttpContext.RequestAborted);

            if (user == null)
            {
                var gitHubEmail = string.IsNullOrWhiteSpace(gitHubUserInfo.Email)
                    ? (await this.gitHubOAuthService.GetGitHubEmailsAsync(githubToken, this.HttpContext.RequestAborted)).FirstOrDefault(e => e.Primary && e.Verified)?.Email
                    : gitHubUserInfo.Email;

                if (!string.IsNullOrWhiteSpace(gitHubEmail))
                {
                    user = await this.userRepository.GetByEmailAsync(gitHubEmail, [user => user.RefreshTokens], this.HttpContext.RequestAborted);
                }

                if (user != null)
                {
                    user.LinkedAccounts.Add(new LinkedAccount
                    {
                        Id = gitHubUserInfo.Id.ToString(CultureInfo.InvariantCulture),
                        LinkedAccountType = LinkedAccountType.GitHub
                    });

                    user.EmailConfirmed = user.EmailConfirmed || !string.IsNullOrEmpty(gitHubEmail);

                    var updated = await this.userRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

                    if (updated == 0)
                    {
                        return this.InternalServerError("Unable to link GitHub account to existing user.");
                    }
                }
                else
                {
                    var newUser = new User
                    {
                        UserName = gitHubUserInfo.Login,
                        Email = gitHubEmail,
                        EmailConfirmed = !string.IsNullOrEmpty(gitHubEmail),
                        LinkedAccounts =
                    [
                        new LinkedAccount
                        {
                            Id = gitHubUserInfo.Id.ToString(CultureInfo.InvariantCulture),
                            LinkedAccountType = LinkedAccountType.GitHub
                        }
                    ]
                    };

                    var createResult = await this.userRepository.CreateUserWithoutPasswordAsync(newUser, this.HttpContext.RequestAborted);

                    if (!createResult.Succeeded)
                    {
                        return this.BadRequest([.. createResult.Errors.Select(e => e.Description)]);
                    }

                    user = newUser;

                    // Optionally send confirmation email if email is not confirmed
                    if (!newUser.EmailConfirmed && !string.IsNullOrWhiteSpace(newUser.Email))
                    {
                        await this.SendConfirmEmailLink(newUser);
                    }
                }
            }

            var token = await this.GenerateAndSaveAccessAndRefreshTokensAsync(user, loginRequest.DeviceId);
            var userToReturn = UserDto.FromEntity(user);

            return this.Ok(
                new LoginResponse
                {
                    Token = token,
                    User = userToReturn
                });
        }
        catch (HttpRequestException)
        {
            return this.Unauthorized("Unable to validate GitHub token.");
        }
        catch
        {
            return this.InternalServerError("Unable to login with GitHub.");
        }
    }

    [AllowAnonymous]
    [HttpGet("github/callback", Name = nameof(GitHubCallback))]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult GitHubCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return this.Redirect($"{this.authSettings.UIBaseUrl}#/auth/github/callback?error=missing_code");
        }

        var redirectUrl = $"{this.authSettings.UIBaseUrl}#/auth/github/callback?code={Uri.EscapeDataString(code)}";

        if (!string.IsNullOrWhiteSpace(state))
        {
            redirectUrl += $"&state={Uri.EscapeDataString(state)}";
        }

        return this.Redirect(redirectUrl);
    }

    [AllowAnonymous]
    [HttpGet("google/callback", Name = nameof(GoogleCallback))]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return this.Redirect($"{this.authSettings.UIBaseUrl}#/auth/google/callback?error=missing_code");
        }

        var redirectUrl = $"{this.authSettings.UIBaseUrl}#/auth/google/callback?code={Uri.EscapeDataString(code)}";

        if (!string.IsNullOrWhiteSpace(state))
        {
            redirectUrl += $"&state={Uri.EscapeDataString(state)}";
        }

        return this.Redirect(redirectUrl);
    }

    /// <summary>
    /// Logs the user out by revoking all refresh tokens and clearing the refresh token cookie.
    /// </summary>
    /// <returns>No content.</returns>
    /// <response code="204">No content.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If provided login information is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [HttpPost("logout", Name = nameof(LogoutAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> LogoutAsync()
    {
        if (!this.User.TryGetUserId(out var userId) || userId == null)
        {
            return this.Unauthorized("You must be logged in to log out.");
        }

        var user = await this.userRepository.GetByIdAsync(userId.Value, [u => u.RefreshTokens], this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound("User not found.");
        }

        await this.jwtTokenService.RevokeAllRefreshTokensForUserAsync(user, this.HttpContext.RequestAborted);
        this.Response.Cookies.Delete(CookieKeys.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            IsEssential = true,
            Domain = this.authSettings.CookieDomain
        });

        return this.NoContent();
    }

    /// <summary>
    /// Refreshes a user's access token. Refresh token is stored in a cookie.
    /// </summary>
    /// <param name="refreshTokenRequest">The refresh token request.</param>
    /// <returns>A new set of tokens.</returns>
    /// <response code="200">If the token was refreshed.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If provided token pair was invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [AllowAnonymous]
    [HttpPost("refreshToken", Name = nameof(RefreshTokenAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        if (refreshTokenRequest == null)
        {
            return this.BadRequest();
        }

        // CSRF protection using Double Submit Cookie pattern. This is the only endpoint that requires CSRF token as it is the only one that uses cookies.
        // This isn't really needed as the device id is effectively a CSRF token, but it's a good practice to have it.
        var csrfHeader = this.Request.Headers[AppHeaderNames.CsrfToken].ToString();
        var csrfCookie = this.Request.Cookies[CookieKeys.CsrfToken];

        if (string.IsNullOrEmpty(csrfHeader) || csrfHeader != csrfCookie)
        {
            return this.Unauthorized("CSRF token mismatch.");
        }

        if (!this.Request.Cookies.TryGetValue(CookieKeys.RefreshToken, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return this.Unauthorized("Missing refresh token cookie");
        }

        var (isTokenEligibleForRefresh, user) = await this.jwtTokenService.IsTokenEligibleForRefreshAsync(
            refreshToken,
            refreshTokenRequest.DeviceId,
            this.HttpContext.RequestAborted);

        if (!isTokenEligibleForRefresh || user == null)
        {
            return this.Unauthorized("Invalid token.");
        }

        var token = await this.GenerateAndSaveAccessAndRefreshTokensAsync(user, refreshTokenRequest.DeviceId);

        return this.Ok(
            new RefreshTokenResponse
            {
                Token = token
            });
    }



    private async Task<string> GenerateAndSaveAccessAndRefreshTokensAsync(User user, string deviceId)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        var token = this.jwtTokenService.GenerateJwtTokenForUser(user);
        var refreshToken = await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, deviceId, this.HttpContext.RequestAborted);

        // Set the refresh token cookie
        this.Response.Cookies.Append(CookieKeys.RefreshToken, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Always require HTTPS
            SameSite = SameSiteMode.None, // Required for cross-origin requests
            Expires = DateTimeOffset.UtcNow.AddMinutes(this.authSettings.RefreshTokenExpirationTimeInMinutes),
            IsEssential = true, // Mark as essential for GDPR compliance
            Domain = this.authSettings.CookieDomain // Don't set domain to restrict to exact host
        });

        // Generate and set CSRF token cookie for Double Submit Cookie pattern
        var csrfToken = Guid.NewGuid().ToString();
        this.Response.Cookies.Append(CookieKeys.CsrfToken, csrfToken, new CookieOptions
        {
            HttpOnly = false, // Must be false so JavaScript can read it
            Secure = true, // Always require HTTPS
            SameSite = SameSiteMode.None, // Required for cross-origin requests
            Expires = DateTimeOffset.UtcNow.AddMinutes(this.authSettings.RefreshTokenExpirationTimeInMinutes),
            IsEssential = true, // Mark as essential for GDPR compliance
            Domain = this.authSettings.CookieDomain // Set domain to allow cross-origin requests
        });

        return token;
    }

    private async Task SendConfirmEmailLink(User user)
    {
        if (user == null || string.IsNullOrWhiteSpace(user.Email))
        {
            throw new ArgumentException("User must not be null and must have a valid email.");
        }

        var emailToken = await this.userRepository.UserManager.GenerateEmailConfirmationTokenAsync(user);
        var confLink = $"{this.authSettings.UIBaseUrl}#/confirm-email?token={Uri.EscapeDataString(emailToken)}&email={Uri.EscapeDataString(user.Email)}";

        var (plainTextMessage, htmlMessage) = await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confLink, this.HttpContext.RequestAborted);
        await this.emailService.SendEmailToUserAsync(user, "Welcome to DerpCode - Confirm Your Email! 🎉", plainTextMessage, htmlMessage, this.HttpContext.RequestAborted);
    }
}