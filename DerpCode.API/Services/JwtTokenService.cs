using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DerpCode.API.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IUserRepository userRepository;

    private readonly AuthenticationSettings authSettings;

    public JwtTokenService(IUserRepository userRepository, IOptions<AuthenticationSettings> authSettings)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.authSettings = authSettings?.Value ?? throw new ArgumentNullException(nameof(authSettings));
    }

    public async Task<(bool, User?)> IsTokenEligibleForRefreshAsync(string token, string refreshToken, string deviceId, CancellationToken cancellationToken = default)
    {
        // Still validate the passed in token, but ignore its expiration date by setting validate lifetime to false
        var validationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.authSettings.APISecret)),
            RequireSignedTokens = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            RequireExpirationTime = true,
            ValidateLifetime = false,
            ValidAudience = this.authSettings.TokenAudience,
            ValidIssuer = this.authSettings.TokenIssuer
        };

        ClaimsPrincipal tokenClaims;

        try
        {
            var result = await new JwtSecurityTokenHandler().ValidateTokenAsync(token, validationParameters);

            if (result == null || !result.IsValid)
            {
                return (false, null);
            }

            tokenClaims = new ClaimsPrincipal(result.ClaimsIdentity);
        }
        catch (SecurityTokenException)
        {
            return (false, null);
        }

        var userIdClaim = tokenClaims.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return (false, null);
        }

        var user = await this.userRepository.GetByIdAsync(userId, [user => user.RefreshTokens], cancellationToken);

        if (user == null)
        {
            return (false, null);
        }

        user.RefreshTokens.RemoveAll(token => token.Expiration <= DateTime.UtcNow);

        var currentRefreshToken = user.RefreshTokens.FirstOrDefault(token => token.DeviceId == deviceId && token.Token == refreshToken);

        if (currentRefreshToken == null)
        {
            await this.userRepository.SaveChangesAsync(cancellationToken);
            return (false, null);
        }

        return (true, user);
    }

    public async Task<string> GenerateAndSaveRefreshTokenForUserAsync(User user, string deviceId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentNullException(nameof(deviceId));
        }

        user.RefreshTokens.RemoveAll(token => token.Expiration <= DateTime.UtcNow || token.DeviceId == deviceId);

        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var refreshToken = Convert.ToBase64String(randomNumber).ConvertToBase64Url();

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            Expiration = DateTimeOffset.UtcNow.AddMinutes(this.authSettings.RefreshTokenExpirationTimeInMinutes),
            DeviceId = deviceId
        });

        await this.userRepository.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }

    public string GenerateJwtTokenForUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer),
                new(ClaimTypes.Name, user.UserName ?? throw new InvalidOperationException("UserName cannot be null")),
                new(ClaimTypes.Email, user.Email ?? throw new InvalidOperationException("Email cannot be null")),
                new(AppClaimTypes.EmailVerified, user.EmailConfirmed.ToString(), ClaimValueTypes.Boolean)
            };

        if (user.FirstName != null)
        {
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
        }

        if (user.LastName != null)
        {
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
        }

        if (user.UserRoles != null)
        {
            foreach (var role in user.UserRoles.Select(r => r.Role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role ?? throw new InvalidOperationException("Role cannot be null")));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.authSettings.APISecret));

        if (key.KeySize < 512)
        {
            throw new ArgumentException("API Secret must be longer");
        }

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(this.authSettings.TokenExpirationTimeInMinutes),
            NotBefore = DateTime.UtcNow,
            SigningCredentials = creds,
            Audience = this.authSettings.TokenAudience,
            Issuer = this.authSettings.TokenIssuer
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public async Task RevokeAllRefreshTokensForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await this.userRepository.GetByIdAsync(userId, [user => user.RefreshTokens], cancellationToken);

        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }

        // Clear all refresh tokens
        user.RefreshTokens.Clear();
        await this.userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeRefreshTokenForDeviceAsync(int userId, string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentNullException(nameof(deviceId));
        }

        var user = await this.userRepository.GetByIdAsync(userId, [user => user.RefreshTokens], cancellationToken);

        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }

        // Remove the refresh token for the specified device
        user.RefreshTokens.RemoveAll(token => token.DeviceId == deviceId);
        await this.userRepository.SaveChangesAsync(cancellationToken);
    }
}