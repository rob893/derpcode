using System;
using System.Collections.Generic;

namespace DerpCode.API.Models.Settings;

public sealed record AuthenticationSettings
{
    /// <summary>
    /// The API secret of this API
    /// </summary>
    public string APISecret { get; init; } = default!;

    /// <summary>
    /// The value of the audience claim for tokens created by this API
    /// </summary>
    public string TokenAudience { get; init; } = default!;

    /// <summary>
    /// The value of the issuer claim for tokens created by this API
    /// </summary>
    public string TokenIssuer { get; init; } = default!;

    /// <summary>
    /// This is the amount of time a token issued by this API FOR THIS API (not tokens issued from the API for other APIs using the TokensController) is good for.
    /// For example, on ad authentication, a token for use by this API is created and should have a short lifespan so the calling API can then use that token 
    /// to call the TokensController with to generate a token for its use.
    /// </summary>
    public int TokenExpirationTimeInMinutes { get; init; }

    public int RefreshTokenExpirationTimeInMinutes { get; init; }

    /// <summary>
    /// List of Google OAuth Client IDs to validate Google Tokens against
    /// </summary>
    public List<string> GoogleOAuthAudiences { get; init; } = [];

    /// <summary>
    /// Google OAuth Client ID for exchanging authorization codes
    /// </summary>
    public string GoogleOAuthClientId { get; init; } = default!;

    /// <summary>
    /// Google OAuth Client Secret for exchanging authorization codes
    /// </summary>
    public string GoogleOAuthClientSecret { get; init; } = default!;

    /// <summary>
    /// Google OAuth redirect URI for exchanging authorization codes. Should be the api callback url.
    /// </summary>
    public Uri GoogleOAuthRedirectUri { get; init; } = default!;

    /// <summary>
    /// GitHub OAuth Client ID for validating GitHub access tokens
    /// </summary>
    public string GitHubOAuthClientId { get; init; } = default!;

    /// <summary>
    /// GitHub OAuth Client Secret for validating GitHub access tokens
    /// </summary>
    public string GitHubOAuthClientSecret { get; init; } = default!;

    public string? CookieDomain { get; init; }

    public Uri UIBaseUrl { get; init; } = default!;
}