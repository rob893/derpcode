using System;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/problems/{problemId}")]
[ApiVersion("1.0")]
public sealed class ProblemSubmissionsController : ServiceControllerBase
{
    private readonly ILogger<ProblemSubmissionsController> logger;

    private readonly ICodeExecutionService codeExecutionService;

    private readonly IProblemRepository problemRepository;

    private readonly IProblemSubmissionRepository problemSubmissionRepository;

    private readonly ICurrentUserService currentUserService;

    public ProblemSubmissionsController(
        ILogger<ProblemSubmissionsController> logger,
        ICorrelationIdService correlationIdService,
        ICodeExecutionService codeExecutionService,
        IProblemRepository problemRepository,
        IProblemSubmissionRepository problemSubmissionRepository,
        ICurrentUserService currentUserService)
            : base(correlationIdService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
        this.problemSubmissionRepository = problemSubmissionRepository ?? throw new ArgumentNullException(nameof(problemSubmissionRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    [HttpGet("submissions/{submissionId}", Name = nameof(GetProblemSubmissionAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemSubmissionDto>> GetProblemSubmissionAsync([FromRoute] int problemId, [FromRoute] int submissionId)
    {
        var submission = await this.problemSubmissionRepository.GetByIdAsync(submissionId, track: false, this.HttpContext.RequestAborted);

        if (submission == null || submission.ProblemId != problemId)
        {
            return this.NotFound($"Submission with ID {submissionId} for problem with ID {problemId} not found");
        }

        if (!this.currentUserService.IsUserAuthorizedForResource(submission))
        {
            return this.Forbidden("You can only see your own submissions.");
        }

        return this.Ok(ProblemSubmissionDto.FromEntity(submission));
    }

    [HttpPost("submissions", Name = nameof(SubmitSolutionAsync))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProblemSubmissionDto>> SubmitSolutionAsync([FromRoute] int problemId, [FromBody] ProblemSubmissionRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserCode))
        {
            return this.BadRequest("User code and language are required");
        }

        if (!this.User.TryGetUserId(out var userId))
        {
            return this.Forbidden();
        }

        if (!this.User.TryGetEmailVerified(out var emailVerified) || !emailVerified.Value)
        {
            return this.Forbidden("You must verify your email before submitting solutions.");
        }

        var problem = await this.problemRepository.GetByIdAsync(problemId, track: true, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return this.NotFound($"Problem with ID {problemId} not found");
        }

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == request.Language);
        if (driver == null)
        {
            return this.BadRequest($"No driver template found for language {request.Language}");
        }

        try
        {
            var result = await this.codeExecutionService.RunCodeAsync(userId.Value, request.UserCode, request.Language, problem, this.HttpContext.RequestAborted);

            problem.ProblemSubmissions.Add(result);
            var updated = await this.problemRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

            if (updated == 0)
            {
                return this.InternalServerError("Failed to save submission. Please try again later.");
            }

            return this.CreatedAtRoute(nameof(GetProblemSubmissionAsync), new { problemId = result.ProblemId, submissionId = result.Id }, ProblemSubmissionDto.FromEntity(result));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code");
            return this.InternalServerError();
        }
    }

    [HttpPost("run", Name = nameof(RunSolutionAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProblemSubmissionDto>> RunSolutionAsync([FromRoute] int problemId, [FromBody] ProblemSubmissionRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserCode))
        {
            return this.BadRequest("User code and language are required");
        }

        if (!this.User.TryGetUserId(out var userId))
        {
            return this.Forbidden();
        }

        var problem = await this.problemRepository.GetByIdAsync(problemId, track: false, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return this.NotFound($"Problem with ID {problemId} not found");
        }

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == request.Language);
        if (driver == null)
        {
            return this.BadRequest($"No driver template found for language {request.Language}");
        }

        try
        {
            var result = await this.codeExecutionService.RunCodeAsync(userId.Value, request.UserCode, request.Language, problem, this.HttpContext.RequestAborted);
            return this.Ok(ProblemSubmissionDto.FromEntity(result));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code");
            return this.InternalServerError();
        }
    }
}