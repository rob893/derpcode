using System.Threading.RateLimiting;
using DerpCode.API.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace DerpCode.API.ApplicationStartup.ServiceCollectionExtensions;

public static class RateLimiterServiceCollectionExtensions
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IServiceCollection AddRateLimiterServices(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var path = httpContext.Request.Path.ToString();
                var method = httpContext.Request.Method;

                // Health checks are hit frequently by probes and test harnesses.
                // Avoid rate-limiting them to prevent flakiness and false negatives.
                if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.GetPartitionKey(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10_000,
                            QueueLimit = 0,
                            Window = TimeSpan.FromSeconds(1)
                        });
                }

                if (path.StartsWith("/api/v1/auth/register", StringComparison.OrdinalIgnoreCase))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.GetPartitionKey(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(15)
                        });
                }

                if (path.StartsWith("/api/v1/auth/", StringComparison.OrdinalIgnoreCase))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.GetPartitionKey(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 25,
                            QueueLimit = 0,
                            Window = TimeSpan.FromSeconds(60)
                        });
                }

                if (
                    path.StartsWith("/api/v1/problems", StringComparison.OrdinalIgnoreCase) &&
                    (path.EndsWith("/submissions", StringComparison.OrdinalIgnoreCase) || path.EndsWith("/run", StringComparison.OrdinalIgnoreCase)) &&
                    method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.GetPartitionKey(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 8,
                            QueueLimit = 0,
                            Window = TimeSpan.FromSeconds(60)
                        });
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.GetPartitionKey(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 50,
                        QueueLimit = 0,
                        Window = TimeSpan.FromSeconds(15)
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                var key = context.HttpContext.GetPartitionKey();

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<HttpContext>>();
                logger.LogWarning("Rate limit exceeded for partition key: {Key}", key);

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers.RetryAfter = "15";
                context.HttpContext.Response.ContentType = "application/json";

                var problemDetails = new ProblemDetailsWithErrors("Rate limit exceeded. Please try again later.", context.HttpContext.Response.StatusCode, context.HttpContext.Request);
                var jsonResponse = JsonSerializer.Serialize(problemDetails, jsonOptions);

                await context.HttpContext.Response.WriteAsync(jsonResponse, cancellationToken);
            };
        });

        return services;
    }

    internal static string GetPartitionKey(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.User.Identity?.Name ?? context.GetIpAddress() ?? "anonymous";
    }

    // Intentionally does NOT read raw forwarding headers like X-Forwarded-For: those values are
    // attacker-controlled when the request is not coming through a trusted proxy and would let a
    // bot rotate fake source IPs to evade per-IP throttles. The ForwardedHeaders middleware
    // (configured in Program.cs with explicit KnownProxies/KnownNetworks) is responsible for
    // rewriting RemoteIpAddress to the real client IP when a trusted proxy is in front.
    internal static string? GetIpAddress(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Connection.RemoteIpAddress?.ToString();
    }
}