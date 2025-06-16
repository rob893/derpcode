using System;
using DerpCode.API.Services.Core;
using Microsoft.Extensions.DependencyInjection;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

/// <summary>
/// Extension methods for registering core services
/// </summary>
public static class CoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds core services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ICorrelationIdService, CorrelationIdService>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddScoped<ICodeExecutionService, CodeExecutionService>();

        return services;
    }
}
