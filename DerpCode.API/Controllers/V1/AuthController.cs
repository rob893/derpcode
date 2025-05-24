using System;
using System.Linq;
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
[AllowAnonymous]
[ApiController]
public sealed class AuthController : ServiceControllerBase
{
    private readonly IUserRepository userRepository;

    private readonly IJwtTokenService jwtTokenService;

    private readonly AuthenticationSettings authSettings;

    public AuthController(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IOptions<AuthenticationSettings> authSettings,
        ICorrelationIdService correlationIdService)
            : base(correlationIdService)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
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

        // await this.SendConfirmEmailLink(user);

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
    [HttpPost("login", Name = nameof(LoginAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null)
        {
            return this.BadRequest();
        }

        var user = await this.userRepository.GetByUsernameAsync(loginRequest.Username, [user => user.RefreshTokens], this.HttpContext.RequestAborted);

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
    /// Refreshes a user's access token. Refresh token is stored in a cookie.
    /// </summary>
    /// <param name="refreshTokenRequest">The refresh token request.</param>
    /// <returns>A new set of tokens.</returns>
    /// <response code="200">If the token was refreshed.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If provided token pair was invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [HttpPost("refreshToken", Name = nameof(RefreshTokenAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        if (refreshTokenRequest == null)
        {
            return this.BadRequest();
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

    /// <summary>
    /// Sends a link to reset password if a user forgot.
    /// </summary>
    /// <param name="request">The forgot password request.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the password reset link was sent.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [HttpPost("forgotPassword", Name = nameof(ForgotPasswordAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        if (request == null)
        {
            return this.BadRequest();
        }

        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return this.BadRequest();
        }

        var token = await this.userRepository.UserManager.GeneratePasswordResetTokenAsync(user);

        var confLink = $"{this.authSettings.ForgotPasswordCallbackUrl}?token={token}&email={user.Email}";
        // TODO: implement later
        // await this.emailService.SendEmailAsync(user.Email, "Reset your password", $"Please click {confLink} to reset your password");

        return this.NoContent();
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="request">The reset password request.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the password was reset.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [HttpPost("resetPassword", Name = nameof(ResetPasswordAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        if (request == null)
        {
            return this.BadRequest();
        }

        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return this.BadRequest();
        }

        var result = await this.userRepository.UserManager.ResetPasswordAsync(user, request.Token, request.Password);

        if (!result.Succeeded)
        {
            return this.BadRequest([.. result.Errors.Select(e => e.Description)]);
        }

        return this.NoContent();
    }

    /// <summary>
    /// Confirms a user's email.
    /// </summary>
    /// <param name="request">The confirm email request.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the email was confirmed.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [HttpPost("confirmEmail", Name = nameof(ConfirmEmailAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request)
    {
        if (request == null)
        {
            return this.BadRequest();
        }

        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return this.BadRequest("Unable to confirm email.");
        }

        var decoded = request.Token.ConvertToStringFromBase64Url();

        var confirmResult = await this.userRepository.UserManager.ConfirmEmailAsync(user, decoded);

        if (!confirmResult.Succeeded)
        {
            return this.BadRequest([.. confirmResult.Errors.Select(e => e.Description)]);
        }

        return this.NoContent();
    }

    private async Task<string> GenerateAndSaveAccessAndRefreshTokensAsync(User user, string deviceId)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        var token = this.jwtTokenService.GenerateJwtTokenForUser(user);
        var refreshToken = await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, deviceId, this.HttpContext.RequestAborted);

        this.Response.Cookies.Append(CookieKeys.RefreshToken, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(this.authSettings.RefreshTokenExpirationTimeInMinutes)
        });

        return token;
    }

    // private async Task SendConfirmEmailLink(User user)
    // {
    //     var emailToken = await this.userRepository.UserManager.GenerateEmailConfirmationTokenAsync(user);
    //     var encoded = emailToken.ConvertToBase64Url();
    //     var confLink = $"{this.authSettings.ConfirmEmailCallbackUrl}?token={encoded}&email={user.Email}";
    //     await this.emailService.SendEmailAsync(user.Email, "Confirm your email", $"Please click {confLink} to confirm email");
    // }
}