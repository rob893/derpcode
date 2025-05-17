using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[AllowAnonymous]
[ApiVersion("2")]
[Route("api/v{version:apiVersion}/test")]
public class TestV2Controller : ControllerBase
{
    private readonly ILogger<TestV2Controller> logger;

    public TestV2Controller(ILogger<TestV2Controller> logger)
    {
        this.logger = logger;
    }

    [HttpGet("ping", Name = nameof(Ping))]
    public ActionResult<string> Ping()
    {
        this.logger.LogInformation("TEST V2");
        return this.Ok("pong V2");
    }
}
