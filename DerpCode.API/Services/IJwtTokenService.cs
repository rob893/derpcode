using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Services;

public interface IJwtTokenService
{
    Task<(bool IsEligible, User? User)> IsTokenEligibleForRefreshAsync(string refreshToken, string deviceId, CancellationToken cancellationToken = default);

    Task<string> GenerateAndSaveRefreshTokenForUserAsync(User user, string deviceId, CancellationToken cancellationToken = default);

    string GenerateJwtTokenForUser(User user);

    Task RevokeAllRefreshTokensForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenForDeviceAsync(int userId, string deviceId, CancellationToken cancellationToken = default);
}