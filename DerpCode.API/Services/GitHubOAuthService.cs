using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DerpCode.API.Models;
using DerpCode.API.Models.Settings;
using Microsoft.Extensions.Options;

namespace DerpCode.API.Services;

/// <summary>
/// Service for validating GitHub OAuth tokens and retrieving user information
/// </summary>
public sealed class GitHubOAuthService : IGitHubOAuthService
{
    private readonly IHttpClientFactory httpClientFactory;

    private readonly AuthenticationSettings authSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubOAuthService"/> class
    /// </summary>
    /// <param name="httpClientFactory">HTTP client factory for making API calls</param>
    /// <param name="authSettings">Authentication settings</param>
    public GitHubOAuthService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthenticationSettings> authSettings)
    {
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        this.authSettings = authSettings?.Value ?? throw new ArgumentNullException(nameof(authSettings));
    }

    public async Task<string> ExchangeCodeForGithubAccessTokenAsync(string code, CancellationToken cancellationToken)
    {
        using var client = this.httpClientFactory.CreateClient();

        using var encodedContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", this.authSettings.GitHubOAuthClientId },
            { "client_secret", this.authSettings.GitHubOAuthClientSecret },
            { "code", code }
        });
        var response = await client.PostAsync(new Uri("https://github.com/login/oauth/access_token"), encodedContent, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var query = HttpUtility.ParseQueryString(content);

        return query["access_token"] ?? throw new InvalidOperationException("GitHub access token not found in response.");
    }

    public async Task<GitHubUser> GetGitHubUser(string accessToken, CancellationToken cancellationToken)
    {
        using var client = this.httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("DerpCode");

        var response = await client.GetStringAsync(new Uri("https://api.github.com/user"), cancellationToken);
        return JsonSerializer.Deserialize<GitHubUser>(response)
            ?? throw new InvalidOperationException("Failed to deserialize GitHub user information.");
    }
}