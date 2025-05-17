using Docker.DotNet;
using DerpCode.API.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class DockerServiceCollectionExtensions
{
    public static IServiceCollection AddDockerServices(this IServiceCollection services)
    {
        services.AddSingleton<IDockerClient>(_ =>
        {
            var dockerUri = IsRunningOnWindows()
                ? "npipe://./pipe/docker_engine"
                : "unix:///var/run/docker.sock";

            return new DockerClientConfiguration(new Uri(dockerUri))
                .CreateClient();
        });

        services.AddScoped<ICodeExecutionService, CodeExecutionService>();

        return services;
    }

    private static bool IsRunningOnWindows()
    {
        return OperatingSystem.IsWindows();
    }
}
