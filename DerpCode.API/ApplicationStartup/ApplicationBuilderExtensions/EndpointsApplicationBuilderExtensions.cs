using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using DerpCode.API.Constants;

namespace DerpCode.API.ApplicationStartup.ApplicationBuilderExtensions;

public static class EndpointsApplicationBuilderExtensions
{
    public static IApplicationBuilder UseAndConfigureEndpoints(this IApplicationBuilder app, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(config);

        app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks(ApplicationSettings.HealthCheckEndpoint, new HealthCheckOptions()
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapHealthChecks(ApplicationSettings.LivenessHealthCheckEndpoint, new HealthCheckOptions()
                {
                    Predicate = (check) => !check.Tags.Contains(HealthCheckTags.Dependency),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapControllers();
            });

        return app;
    }
}