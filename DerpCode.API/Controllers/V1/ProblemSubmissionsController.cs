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

    /// <summary>
    /// Gets a specific submission for a problem.
    /// </summary>
    /// <param name="problemId">The ID of the problem.</param>
    /// <param name="submissionId">The ID of the submission to retrieve.</param>
    /// <returns>The submission with the specified ID.</returns>
    /// <response code="200">Returns the submission.</response>
    /// <response code="403">If the user is not authorized to view this submission.</response>
    /// <response code="404">If the submission is not found.</response>
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

    /// <summary>
    /// Submits a solution for a problem.
    /// </summary>
    /// <param name="problemId">The ID of the problem to submit a solution for.</param>
    /// <param name="request">The submission request containing the solution code.</param>
    /// <returns>The submission result.</returns>
    /// <response code="201">Returns the submission result.</response>
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

    /// <summary>
    /// Runs a solution for a problem without submitting it.
    /// </summary>
    /// <param name="problemId">The ID of the problem to run the solution for.</param>
    /// <param name="request">The submission request containing the solution code.</param>
    /// <returns>The run result.</returns>
    /// <response code="200">Returns the run result.</response>
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