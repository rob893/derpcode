using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DerpCode.API.Constants;
using DerpCode.API.Extensions;
using DerpCode.API.Middleware;
using DerpCode.API.Models.Settings;

namespace DerpCode.API.ApplicationStartup.ApplicationBuilderExtensions;

public static class SwaggerApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAndConfigureSwagger(this IApplicationBuilder app, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var settings = config.GetSection(ConfigurationKeys.Swagger).Get<SwaggerSettings>();

        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        if (!settings.Enabled)
        {
            return app;
        }

        app.UseMiddleware<SwaggerBasicAuthMiddleware>()
            .UseSwagger()
            .UseSwaggerUI(
                options =>
                {
                    var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.RoutePrefix = "swagger";
                        options.SwaggerEndpoint(
                            $"{description.GroupName}/swagger.json",
                            $"{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName} {description.GroupName}");
                        options.DocumentTitle = $"{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName} - {config.GetEnvironment()}";
                    }
                });

        return app;
    }
}