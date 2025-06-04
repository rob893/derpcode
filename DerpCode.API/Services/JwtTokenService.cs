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

    public async Task<(bool IsEligible, User? User)> IsTokenEligibleForRefreshAsync(string refreshToken, string deviceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        var refreshTokens = await this.userRepository.GetRefreshTokensForDeviceAsync(deviceId, track: true, cancellationToken);

        if (refreshTokens.Count == 0)
        {
            return (false, null);
        }

        var refreshTokenObj = refreshTokens.FirstOrDefault(token => VerifyTokenHash(refreshToken, token.TokenHash, token.TokenSalt));

        if (refreshTokenObj == null)
        {
            return (false, null);
        }

        var user = refreshTokenObj.User;

        if (user == null)
        {
            return (false, null);
        }

        var expiredTokens = user.RefreshTokens.Where(token => token.Expiration <= DateTimeOffset.UtcNow).ToList();
        if (expiredTokens.Count > 0)
        {
            user.RefreshTokens.RemoveAll(token => token.Expiration <= DateTimeOffset.UtcNow);
            await this.userRepository.SaveChangesAsync(cancellationToken);
        }

        if (refreshTokenObj.Expiration <= DateTimeOffset.UtcNow)
        {
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

        user.RefreshTokens.RemoveAll(token => token.Expiration <= DateTimeOffset.UtcNow || token.DeviceId == deviceId);

        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var refreshToken = Convert.ToBase64String(randomNumber).ConvertToBase64Url();

        CreateTokenHash(refreshToken, out var tokenHash, out var tokenSalt);

        user.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = tokenHash,
            TokenSalt = tokenSalt,
            CreatedAt = DateTimeOffset.UtcNow,
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
        var user = await this.userRepository.GetByIdAsync(userId, [user => user.RefreshTokens], cancellationToken) ?? throw new ArgumentException($"User with ID {userId} not found.");
        await this.RevokeAllRefreshTokensForUserAsync(user, cancellationToken);
    }

    public async Task RevokeAllRefreshTokensForUserAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.RefreshTokens.Clear();
        await this.userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeRefreshTokenForDeviceAsync(int userId, string deviceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentNullException(nameof(deviceId));
        }

        var user = await this.userRepository.GetByIdAsync(userId, [user => user.RefreshTokens], cancellationToken) ?? throw new ArgumentException($"User with ID {userId} not found.");

        // Remove the refresh token for the specified device
        user.RefreshTokens.RemoveAll(token => token.DeviceId == deviceId);
        await this.userRepository.SaveChangesAsync(cancellationToken);
    }

    private static void CreateTokenHash(string token, out string tokenHash, out string tokenSalt)
    {
        using var hmac = new HMACSHA512();
        var tokenSaltBytes = hmac.Key;
        var tokenHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));

        tokenHash = Convert.ToBase64String(tokenHashBytes);
        tokenSalt = Convert.ToBase64String(tokenSaltBytes);
    }

    private static bool VerifyTokenHash(string token, string tokenHash, string tokenSalt)
    {
        if (string.IsNullOrWhiteSpace(tokenHash) || string.IsNullOrWhiteSpace(tokenSalt))
        {
            return false;
        }

        var tokenSaltBytes = Convert.FromBase64String(tokenSalt);
        var tokenHashBytes = Convert.FromBase64String(tokenHash);
        using var hmac = new HMACSHA512(tokenSaltBytes);
        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));

        return CryptographicOperations.FixedTimeEquals(computedHash, tokenHashBytes);
    }
}