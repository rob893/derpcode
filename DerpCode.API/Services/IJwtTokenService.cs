using System.Threading.Tasks;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Services;

public interface IJwtTokenService
{
    Task<(bool, User?)> IsTokenEligibleForRefreshAsync(string token, string refreshToken, string deviceId);

    Task<string> GenerateAndSaveRefreshTokenForUserAsync(User user, string deviceId);

    string GenerateJwtTokenForUser(User user);
    
    Task RevokeAllRefreshTokensForUserAsync(int userId);
    
    Task RevokeRefreshTokenForDeviceAsync(int userId, string deviceId);
}