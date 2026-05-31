using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DerpCode.API.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using static DerpCode.API.Utilities.UtilityFunctions;

namespace DerpCode.API.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    // Generic, status-keyed response bodies. Intentionally free of exception type names, table
    // names, dependency identifiers, or any other server-side detail. The correlation ID surfaced
    // by ProblemDetailsWithErrors is the support handle for matching the response to logs.
    private static readonly Dictionary<int, string> safeStatusMessages = new()
    {
        { StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing the request. Please reference the correlationId when contacting support." },
        { StatusCodes.Status504GatewayTimeout, "The request timed out. Please reference the correlationId when contacting support." }
    };

    private const string FallbackSafeMessage = "An error occurred while processing the request. Please reference the correlationId when contacting support.";

    private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;

    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionHandlerMiddleware(RequestDelegate _, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var error = context.Features.Get<IExceptionHandlerFeature>();

        if (error != null)
        {
            var sourceName = GetSourceName();
            var thrownException = error.Error;
            var statusCode = thrownException switch
            {
                TimeoutException => StatusCodes.Status504GatewayTimeout,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // Log the full exception (including stack trace and inner-exception chain) for the
            // operator. The exception object is intentionally NOT copied into the response body
            // because messages from EF Core, Postgres, identity providers, etc. can disclose
            // schema, dependency, and infrastructure details that aid attackers.
            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                this.logger.LogError(thrownException, "{SourceName} unhandled exception", sourceName);
            }
            else
            {
                this.logger.LogWarning(thrownException, "{SourceName} unhandled exception", sourceName);
            }

            if (!safeStatusMessages.TryGetValue(statusCode, out var safeMessage))
            {
                safeMessage = FallbackSafeMessage;
            }

            var problemDetails = new ProblemDetailsWithErrors(safeMessage, statusCode, context.Request);
            var jsonResponse = JsonSerializer.Serialize(problemDetails, this.jsonOptions);

            if (!context.Response.HasStarted)
            {
                await context.Response.WriteAsync(jsonResponse, context.RequestAborted);
            }
        }
    }
}