using DerpCode.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

/// <summary>
/// Extension methods for registering business services
/// </summary>
public static class BusinessServiceCollectionExtensions
{
    /// <summary>
    /// Adds business services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IProblemService, ProblemService>();

        return services;
    }
}
