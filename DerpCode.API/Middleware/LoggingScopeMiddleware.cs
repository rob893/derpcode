using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DerpCode.API.Extensions;
using DerpCode.API.Services.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Middleware;

/// <summary>
/// This middleware adds the correlation id passed in the request into the response.
/// </summary>
public sealed class LoggingScopeMiddleware
{
    private readonly RequestDelegate next;

    private readonly ILogger<LoggingScopeMiddleware> logger;

    public LoggingScopeMiddleware(RequestDelegate next, ILogger<LoggingScopeMiddleware> logger)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdService correlationIdService)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(correlationIdService);

        var scope = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationIdService.CorrelationId
        };

        if (context.User.TryGetUserId(out var userId) && userId != null)
        {
            scope["UserId"] = userId;
        }

        using (this.logger.BeginScope(scope))
        {
            await this.next(context);
        }
    }
}