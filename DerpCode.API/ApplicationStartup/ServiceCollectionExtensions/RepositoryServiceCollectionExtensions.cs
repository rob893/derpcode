using System;
using DerpCode.API.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProblemRepository, ProblemRepository>();
        services.AddScoped<IDriverTemplateRepository, DriverTemplateRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IProblemSubmissionRepository, ProblemSubmissionRepository>();
        services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();

        return services;
    }
}