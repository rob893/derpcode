using System;
using DerpCode.API.Constants;
using DerpCode.API.Models.Settings;
using DerpCode.API.Services.Integrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class GitHubServiceCollectionExtensions
{
    public static IServiceCollection AddGitHubServices(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services.Configure<GitHubSettings>(config.GetSection(ConfigurationKeys.GitHub));

        var githubToken = config[ConfigurationKeys.GitHubPat] ?? throw new InvalidOperationException($"GitHub token not found in configuration with key {ConfigurationKeys.GitHubPat}.");
        services.AddSingleton<IGitHubClient>(new GitHubClient(new ProductHeaderValue(ApplicationSettings.GitHubAppHeader))
        {
            Credentials = new Credentials(githubToken)
        });
        services.AddScoped<IGitHubService, GitHubService>();

        return services;
    }
}