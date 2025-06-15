using System;
using System.Collections.Generic;
using DerpCode.API.Core;
using DerpCode.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Controllers;

public abstract class ServiceControllerBase : ControllerBase
{
    private readonly ICorrelationIdService correlationIdService;

    protected ServiceControllerBase(ICorrelationIdService correlationIdService)
    {
        this.correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    }

    protected string CorrelationId => this.correlationIdService.CorrelationId;

    [NonAction]
    public BadRequestObjectResult BadRequest(string? errorMessage = "Bad request.")
    {
        return this.BadRequest([errorMessage ?? "Bad request."]);
    }

    [NonAction]
    public BadRequestObjectResult BadRequest(IEnumerable<string> errorMessages)
    {
        return base.BadRequest(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status400BadRequest, this.Request));
    }

    [NonAction]
    public UnauthorizedObjectResult Unauthorized(string? errorMessage = "Unauthorized.")
    {
        return this.Unauthorized([errorMessage ?? "Unauthorized."]);
    }

    [NonAction]
    public UnauthorizedObjectResult Unauthorized(IEnumerable<string> errorMessages)
    {
        return base.Unauthorized(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status401Unauthorized, this.Request));
    }

    [NonAction]
    public ObjectResult Forbidden(string? errorMessage = "Forbidden.")
    {
        return this.Forbidden([errorMessage ?? "Forbidden."]);
    }

    [NonAction]
    public ObjectResult Forbidden(IEnumerable<string> errorMessages)
    {
        return base.StatusCode(StatusCodes.Status403Forbidden, new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status403Forbidden, this.Request));
    }

    [NonAction]
    public NotFoundObjectResult NotFound(string? errorMessage = "Resource not found.")
    {
        return this.NotFound([errorMessage ?? "Resource not found."]);
    }

    [NonAction]
    public NotFoundObjectResult NotFound(IEnumerable<string> errorMessages)
    {
        return base.NotFound(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status404NotFound, this.Request));
    }

    [NonAction]
    public ObjectResult InternalServerError(string? errorMessage = "Internal server error.")
    {
        return this.InternalServerError([errorMessage ?? "Internal server error."]);
    }

    [NonAction]
    public ObjectResult InternalServerError(IEnumerable<string> errorMessages)
    {
        return base.StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status500InternalServerError, this.Request));
    }

    [NonAction]
    protected ObjectResult HandleServiceFailureResult<T>(Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException("This method should only be called for failed results.");
        }

        return result.ErrorType switch
        {
            DomainErrorType.NotFound => this.NotFound(result.ErrorMessage),
            DomainErrorType.Validation => this.BadRequest(result.ErrorMessage),
            DomainErrorType.Unauthorized => this.Unauthorized(result.ErrorMessage),
            DomainErrorType.Forbidden => this.Forbidden(result.ErrorMessage),
            DomainErrorType.Conflict => this.Conflict(result.ErrorMessage),
            _ => this.InternalServerError(result.ErrorMessage)
        };
    }
}