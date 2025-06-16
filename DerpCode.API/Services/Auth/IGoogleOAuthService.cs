using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth;

namespace DerpCode.API.Services.Auth;

/// <summary>
/// Service for handling Google OAuth tokens and retrieving user information
/// </summary>
public interface IGoogleOAuthService
{
    Task<string> ExchangeCodeForGoogleIdTokenAsync(string code, CancellationToken cancellationToken);

    Task<GoogleJsonWebSignature.Payload> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken);
}