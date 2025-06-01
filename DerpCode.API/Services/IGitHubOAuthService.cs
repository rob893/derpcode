using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models;

namespace DerpCode.API.Services;

/// <summary>
/// Service for validating GitHub OAuth tokens and retrieving user information
/// </summary>
public interface IGitHubOAuthService
{
    Task<string> ExchangeCodeForGithubAccessTokenAsync(string code, CancellationToken cancellationToken);

    Task<GitHubUser> GetGitHubUser(string accessToken, CancellationToken cancellationToken);
}