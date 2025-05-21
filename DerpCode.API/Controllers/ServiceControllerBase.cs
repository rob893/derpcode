using System;
using System.Collections.Generic;
using DerpCode.API.Core;
using DerpCode.API.Extensions;
using DerpCode.API.Models;
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
    public bool IsUserAuthorizedForResource(IOwnedByUser<int> resource, bool isAdminAuthorized = true)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return (isAdminAuthorized && this.User.IsAdmin()) || (this.User.TryGetUserId(out var userId) && userId == resource.UserId);
    }

    [NonAction]
    public bool IsUserAuthorizedForResource(int userIdInQuestion, bool isAdminAuthorized = true)
    {
        return (isAdminAuthorized && this.User.IsAdmin()) || (this.User.TryGetUserId(out var userId) && userId == userIdInQuestion);
    }

    [NonAction]
    public BadRequestObjectResult BadRequest(string errorMessage = "Bad request.")
    {
        return this.BadRequest([errorMessage]);
    }

    [NonAction]
    public BadRequestObjectResult BadRequest(IEnumerable<string> errorMessages)
    {
        return base.BadRequest(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status400BadRequest, this.Request));
    }

    [NonAction]
    public UnauthorizedObjectResult Unauthorized(string errorMessage = "Unauthorized.")
    {
        return this.Unauthorized([errorMessage]);
    }

    [NonAction]
    public UnauthorizedObjectResult Unauthorized(IEnumerable<string> errorMessages)
    {
        return base.Unauthorized(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status401Unauthorized, this.Request));
    }

    [NonAction]
    public ObjectResult Forbidden(string errorMessage = "Forbidden.")
    {
        return this.Forbidden([errorMessage]);
    }

    [NonAction]
    public ObjectResult Forbidden(IEnumerable<string> errorMessages)
    {
        return base.StatusCode(StatusCodes.Status403Forbidden, new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status403Forbidden, this.Request));
    }

    [NonAction]
    public NotFoundObjectResult NotFound(string errorMessage = "Resource not found.")
    {
        return this.NotFound([errorMessage]);
    }

    [NonAction]
    public NotFoundObjectResult NotFound(IEnumerable<string> errorMessages)
    {
        return base.NotFound(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status404NotFound, this.Request));
    }

    [NonAction]
    public ObjectResult InternalServerError(string errorMessage = "Internal server error.")
    {
        return this.InternalServerError([errorMessage]);
    }

    [NonAction]
    public ObjectResult InternalServerError(IEnumerable<string> errorMessages)
    {
        return base.StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status500InternalServerError, this.Request));
    }
}