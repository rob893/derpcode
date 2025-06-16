using System;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Extensions;
using DerpCode.API.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using static DerpCode.API.Utilities.UtilityFunctions;

namespace DerpCode.API.Middleware;

/// <summary>
/// This middleware adds the correlation id passed in the request into the response.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate next;

    private readonly ILogger<CorrelationIdMiddleware> logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdService correlationIdService)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(correlationIdService);

        var sourceName = GetSourceName();

        if (!context.Request.Headers.TryGetCorrelationId(out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[AppHeaderNames.CorrelationId] = correlationId;
            this.logger.LogDebug("[{SourceName}] No correlation id found in request headers. Generating a new one. It will be added to the response headers.", sourceName);
        }
        else
        {
            this.logger.LogDebug("[{SourceName}] Correlation id found in request headers. It will be added to the response headers.", sourceName);
        }

        correlationIdService.CorrelationId = correlationId;

        context.Response.OnStarting(() =>
        {
            // Remove previous correlation id as passed correlation id always takes priority.
            if (context.Response.Headers.TryGetCorrelationId(out var currentCorrelationId))
            {
                this.logger.LogDebug("[{SourceName}] Correlation id of {CorId} has already been added to response headers. Removing in favor of correlation id from client.", sourceName, currentCorrelationId);
                context.Response.Headers.Remove(AppHeaderNames.CorrelationId);
            }

            context.Response.Headers[AppHeaderNames.CorrelationId] = correlationId;

            this.logger.LogDebug("[{SourceName}] Correlation id added to the response headers.", sourceName);

            return Task.CompletedTask;
        });

        // Process the request within the scope
        await this.next(context);
    }
}