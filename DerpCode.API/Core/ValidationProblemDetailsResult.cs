using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Core;

public sealed class ValidationProblemDetailsResult : IActionResult
{
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var errors = context.ModelState
            .Where(e => e.Value != null && e.Value.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(e => $"{entry.Key}: {e.ErrorMessage}"))
            .ToList();

        var problemDetails = new ProblemDetailsWithErrors(errors, StatusCodes.Status400BadRequest, context.HttpContext.Request)
        {
            Title = "One or more validation errors occurred."
        };

        context.HttpContext.Response.ContentType = "application/json";
        context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var jsonResponse = JsonSerializer.Serialize(problemDetails, this.jsonOptions);

        await context.HttpContext.Response.WriteAsync(jsonResponse);
    }
}