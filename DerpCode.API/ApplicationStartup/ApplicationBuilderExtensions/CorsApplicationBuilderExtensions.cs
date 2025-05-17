using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using DerpCode.API.Constants;

namespace DerpCode.API.ApplicationStartup.ApplicationBuilderExtensions;

public static class CorsApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAndConfigureCors(this IApplicationBuilder app, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        app.UseCors(header =>
            header.WithOrigins(config.GetSection(ConfigurationKeys.CorsAllowedOrigins).Get<string[]>() ?? ["*"])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders(config.GetSection(ConfigurationKeys.CorsExposedHeaders).Get<string[]>() ?? [AppHeaderNames.TokenExpired, AppHeaderNames.CorrelationId]));

        return app;
    }
}