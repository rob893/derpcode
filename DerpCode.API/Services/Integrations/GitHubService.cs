using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Settings;
using DerpCode.API.Services.Core;
using Microsoft.Extensions.Options;
using Octokit;

namespace DerpCode.API.Services.Integrations;

public sealed class GitHubService : IGitHubService
{
    private readonly IGitHubClient gitHubClient;

    private readonly IProblemSeedDataService problemSeedDataService;

    private readonly GitHubSettings gitHubSettings;

    public GitHubService(IGitHubClient gitHubClient, IProblemSeedDataService problemSeedDataService, IOptions<GitHubSettings> gitHubSettings)
    {
        this.gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
        this.problemSeedDataService = problemSeedDataService ?? throw new ArgumentNullException(nameof(problemSeedDataService));
        this.gitHubSettings = gitHubSettings?.Value ?? throw new ArgumentNullException(nameof(gitHubSettings));
    }

    public async Task<string> SyncProblemsFromDatabaseToGithubAsync(CancellationToken cancellationToken = default)
    {
        var (updated, newItems, deleted) = await this.problemSeedDataService.GetUpdatedProblemsToSyncFromDatabaseToGitAsync(cancellationToken);

        if (updated.Count == 0 && newItems.Count == 0 && deleted.Count == 0)
        {
            return string.Empty;
        }

        var baseBranch = this.gitHubSettings.DerpCodeRepositoryBaseBranch;
        var repoOwner = this.gitHubSettings.DerpCodeRepositoryOwner;
        var repo = this.gitHubSettings.DerpCodeRepository;
        var newBranchName = $"sync-problems-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        // Step 1: Get the reference of the base branch
        var masterRef = await this.gitHubClient.Git.Reference.Get(
            this.gitHubSettings.DerpCodeRepositoryOwner,
            this.gitHubSettings.DerpCodeRepository,
            $"heads/{baseBranch}");
        var latestCommitSha = masterRef.Object.Sha;

        // Step 2: Create new branch
        var newBranch = await this.gitHubClient.Git.Reference.Create(repoOwner, repo, new NewReference($"refs/heads/{newBranchName}", latestCommitSha));

        // Step 3: Get the base tree
        var latestCommit = await this.gitHubClient.Git.Commit.Get(repoOwner, repo, latestCommitSha);
        var baseTree = latestCommit.Tree.Sha;

        var changes = new List<NewTreeItem>();
        static string NormalizePath(string path) => path.Replace("\\", "/", StringComparison.Ordinal);

        // Add or update files
        foreach (var kvp in newItems.Concat(updated))
        {
            changes.Add(new NewTreeItem
            {
                Path = NormalizePath(kvp.Key),
                Mode = "100644",
                Type = TreeType.Blob,
                Content = kvp.Value
            });
        }

        // Delete files
        foreach (var path in deleted)
        {
            changes.Add(new NewTreeItem
            {
                Path = NormalizePath(path),
                Mode = "100644",
                Type = TreeType.Blob,
                Sha = null // signals deletion
            });
        }

        // Step 4: Create new tree
        var newTree = new NewTree { BaseTree = baseTree };

        foreach (var item in changes)
        {
            newTree.Tree.Add(item);
        }

        var createdTree = await this.gitHubClient.Git.Tree.Create(repoOwner, repo, newTree);

        // Step 5: Create commit
        var newCommit = new NewCommit("Sync problems from database", createdTree.Sha, latestCommitSha);
        var commit = await this.gitHubClient.Git.Commit.Create(repoOwner, repo, newCommit);

        // Step 6: Update branch to point to new commit
        await this.gitHubClient.Git.Reference.Update(repoOwner, repo, $"heads/{newBranchName}", new ReferenceUpdate(commit.Sha));

        // Step 7: Create PR
        var pr = new NewPullRequest("Sync problems from database", newBranchName, baseBranch)
        {
            Body = "This PR was created automatically to sync problem content from the database."
        };

        var createdPr = await this.gitHubClient.PullRequest.Create(repoOwner, repo, pr);

        return createdPr.HtmlUrl;
    }
}