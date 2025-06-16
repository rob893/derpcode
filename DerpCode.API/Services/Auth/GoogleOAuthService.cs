using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Settings;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace DerpCode.API.Services.Auth;

/// <summary>
/// Service for handling Google OAuth tokens and retrieving user information
/// </summary>
public sealed class GoogleOAuthService : IGoogleOAuthService
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory httpClientFactory;

    private readonly AuthenticationSettings authSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleOAuthService"/> class
    /// </summary>
    /// <param name="httpClientFactory">HTTP client factory for making API calls</param>
    /// <param name="authSettings">Authentication settings</param>
    public GoogleOAuthService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthenticationSettings> authSettings)
    {
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        this.authSettings = authSettings?.Value ?? throw new ArgumentNullException(nameof(authSettings));
    }

    public async Task<string> ExchangeCodeForGoogleIdTokenAsync(string code, CancellationToken cancellationToken)
    {
        using var client = this.httpClientFactory.CreateClient();

        using var encodedContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", this.authSettings.GoogleOAuthClientId },
            { "client_secret", this.authSettings.GoogleOAuthClientSecret },
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", this.authSettings.GoogleOAuthRedirectUri.ToString() }
        });

        var response = await client.PostAsync(new Uri("https://oauth2.googleapis.com/token"), encodedContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Google token exchange failed with status code {response.StatusCode}: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(content, this.jsonSerializerOptions);

        return tokenResponse?.IdToken ?? throw new InvalidOperationException("Google ID token not found in response.");
    }

    public async Task<GoogleJsonWebSignature.Payload> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        return await GoogleJsonWebSignature.ValidateAsync(
            idToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = this.authSettings.GoogleOAuthAudiences
            });
    }

    private sealed record GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = default!;

        [JsonPropertyName("id_token")]
        public string IdToken { get; init; } = default!;

        [JsonPropertyName("token_type")]
        public string TokenType { get; init; } = default!;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("scope")]
        public string Scope { get; init; } = default!;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }
    }
}