using System;
using DerpCode.API.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace DerpCode.API.ApplicationStartup.ApplicationBuilderExtensions;

public static class CorsApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAndConfigureCors(this IApplicationBuilder app, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(config);

        app.UseCors(header =>
            header.WithOrigins(config.GetSection(ConfigurationKeys.CorsAllowedOrigins).Get<string[]>() ?? ["*"])
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders(config.GetSection(ConfigurationKeys.CorsExposedHeaders).Get<string[]>() ?? [AppHeaderNames.TokenExpired, AppHeaderNames.CorrelationId]));

        return app;
    }
}