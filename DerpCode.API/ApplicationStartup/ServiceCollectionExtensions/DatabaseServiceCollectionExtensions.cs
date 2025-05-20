using System;
using DerpCode.API.Constants;
using DerpCode.API.Data;
using DerpCode.API.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services.Configure<MySQLSettings>(config.GetSection(ConfigurationKeys.MySQL));

        var settings = config.GetSection(ConfigurationKeys.MySQL).Get<MySQLSettings>();

        services.AddDbContext<DataContext>(
            dbContextOptions =>
            {
                dbContextOptions
                    .UseMySql(settings.DefaultConnection, ServerVersion.AutoDetect(settings.DefaultConnection), options =>
                    {
                        options.EnableRetryOnFailure();
                        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    });

                if (settings.EnableDetailedErrors)
                {
                    dbContextOptions.EnableDetailedErrors();
                }

                if (settings.EnableSensitiveDataLogging)
                {
                    dbContextOptions.EnableSensitiveDataLogging();
                }
            }
        );

        services.AddTransient<IDatabaseSeeder, DatabaseSeeder>();

        return services;
    }
}