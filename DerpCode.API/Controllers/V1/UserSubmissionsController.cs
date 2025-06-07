using System;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models;
using DerpCode.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/users/{userId}/submissions")]
[ApiVersion("1.0")]
public sealed class UserSubmissionsController : ServiceControllerBase
{
    private readonly ILogger<UserSubmissionsController> logger;

    private readonly ICodeExecutionService codeExecutionService;

    private readonly IProblemRepository problemRepository;

    public UserSubmissionsController(
        ILogger<UserSubmissionsController> logger,
        ICorrelationIdService correlationIdService,
        ICodeExecutionService codeExecutionService,
        IProblemRepository problemRepository)
            : base(correlationIdService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
    }

    [HttpPost(Name = nameof(SubmitSolutionAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SubmissionResult>> SubmitSolutionAsync([FromBody] SubmissionRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserCode))
        {
            return this.BadRequest("User code and language are required");
        }

        var problem = await this.problemRepository.GetByIdAsync(request.ProblemId, track: false, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return this.NotFound($"Problem with ID {request.ProblemId} not found");
        }

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == request.Language);
        if (driver == null)
        {
            return this.BadRequest($"No driver template found for language {request.Language}");
        }

        try
        {
            var result = await this.codeExecutionService.RunCodeAsync(request.UserCode, request.Language, problem, this.HttpContext.RequestAborted);
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code");
            return this.InternalServerError();
        }
    }
}