using System;
using Octokit;

namespace DerpCode.API.Services.Integrations;

public sealed class GitHubService : IGitHubService
{
    private readonly IGitHubClient gitHubClient;

    public GitHubService(IGitHubClient gitHubClient)
    {
        this.gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
    }

    public void Temp()
    {
        throw new NotImplementedException("This method is a temporary placeholder and should not be used in production.");
    }
}