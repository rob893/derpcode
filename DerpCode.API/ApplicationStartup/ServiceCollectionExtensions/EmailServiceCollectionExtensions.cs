using System;
using DerpCode.API.Constants;
using DerpCode.API.Models.Settings;
using DerpCode.API.Services;
using DerpCode.API.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services.Configure<EmailSettings>(config.GetSection(ConfigurationKeys.Email));

        services.AddSingleton<IAcsEmailClientFactory, AcsEmailClientFactory>()
            .AddScoped<IEmailService, AcsEmailService>()
            .AddSingleton<IEmailTemplateService, EmailTemplateService>();

        return services;
    }
}