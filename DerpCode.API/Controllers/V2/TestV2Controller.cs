using System;
using DerpCode.API.Services.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V2;

[ApiController]
[ApiVersion("2")]
[Route("api/v{version:apiVersion}/test")]
public sealed class TestV2Controller : ServiceControllerBase
{
    private readonly ILogger<TestV2Controller> logger;

    public TestV2Controller(ILogger<TestV2Controller> logger, ICorrelationIdService correlationIdService) : base(correlationIdService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Health check endpoint that returns a pong response.
    /// </summary>
    /// <returns>A pong response string.</returns>
    /// <response code="200">Returns the pong response.</response>
    [HttpGet("ping", Name = nameof(Ping))]
    public ActionResult<string> Ping()
    {
        this.logger.LogInformation("TEST V2");
        return this.Ok("pong V2");
    }
}