using System;
using System.Threading.Tasks;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/problems/{problemId}")]
[ApiVersion("1.0")]
public sealed class ProblemSubmissionsController : ServiceControllerBase
{
    private readonly IProblemSubmissionService problemSubmissionService;

    public ProblemSubmissionsController(ICorrelationIdService correlationIdService, IProblemSubmissionService problemSubmissionService)
        : base(correlationIdService)
    {
        this.problemSubmissionService = problemSubmissionService ?? throw new ArgumentNullException(nameof(problemSubmissionService));
    }

    [HttpGet("submissions/{submissionId}", Name = nameof(GetProblemSubmissionAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProblemSubmissionDto>> GetProblemSubmissionAsync([FromRoute] int problemId, [FromRoute] int submissionId)
    {
        var submissionResult = await this.problemSubmissionService.GetProblemSubmissionAsync(problemId, submissionId, this.HttpContext.RequestAborted);

        if (!submissionResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(submissionResult);
        }

        return this.Ok(submissionResult.ValueOrThrow);
    }

    [HttpPost("submissions", Name = nameof(SubmitSolutionAsync))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProblemSubmissionDto>> SubmitSolutionAsync([FromRoute] int problemId, [FromBody] ProblemSubmissionRequest request)
    {
        var submissionResult = await this.problemSubmissionService.SubmitSolutionAsync(problemId, request, this.HttpContext.RequestAborted);

        if (!submissionResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(submissionResult);
        }

        var submission = submissionResult.ValueOrThrow;

        return this.CreatedAtRoute(nameof(GetProblemSubmissionAsync), new { problemId = submission.ProblemId, submissionId = submission.Id }, submission);
    }

    [HttpPost("run", Name = nameof(RunSolutionAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProblemSubmissionDto>> RunSolutionAsync([FromRoute] int problemId, [FromBody] ProblemSubmissionRequest request)
    {
        var runResult = await this.problemSubmissionService.RunSolutionAsync(problemId, request, this.HttpContext.RequestAborted);

        if (!runResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(runResult);
        }

        return this.Ok(runResult.ValueOrThrow);
    }
}