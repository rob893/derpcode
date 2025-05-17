using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[AllowAnonymous]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/test")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> logger;

    public TestController(ILogger<TestController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("ping", Name = nameof(Ping))]
    public ActionResult<string> Ping()
    {
        this.logger.LogInformation("TEST");
        return this.Ok("pong");
    }

    [HttpGet("error", Name = nameof(Error))]
    public ActionResult<string> Error()
    {
        this.logger.LogError("ERROR");
        throw new NotImplementedException("ERROR ENDPOINT!");
    }
}
