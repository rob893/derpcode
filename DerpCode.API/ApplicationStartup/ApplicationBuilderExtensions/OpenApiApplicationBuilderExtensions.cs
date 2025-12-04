using System;
using System.Diagnostics;
using System.Reflection;
using DerpCode.API.Constants;
using DerpCode.API.Extensions;
using DerpCode.API.Middleware;
using DerpCode.API.Models.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;

namespace DerpCode.API.ApplicationStartup.ApplicationBuilderExtensions;

public static class OpenApiApplicationBuilderExtensions
{
    public static WebApplication UseAndConfigureOpenApi(this WebApplication app, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(config);

        var settings = config.GetSection(ConfigurationKeys.OpenApi).Get<OpenApiSettings>();

        ArgumentNullException.ThrowIfNull(settings);

        if (!settings.Enabled)
        {
            return app;
        }

        var openApiSettings = config.GetSection(ConfigurationKeys.OpenApi).Get<OpenApiSettings>();
        var productName = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName;
        var environment = config.GetEnvironment();

        // Apply basic auth middleware for protecting the OpenAPI/Scalar endpoints
        app.UseMiddleware<OpenApiBasicAuthMiddleware>();

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle($"{productName} - {environment}")
                .WithTheme(ScalarTheme.BluePlanet)
                .WithOperationTitleSource(OperationTitleSource.Path)
                .AddPreferredSecuritySchemes("Bearer");
        });

        return app;
    }
}