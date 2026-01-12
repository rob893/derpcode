using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
using DerpCode.API.Models.Responses;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using DerpCode.API.Services.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/problems")]
[ApiVersion("1.0")]
public sealed class ProblemsController : ServiceControllerBase
{
    private readonly IProblemService problemService;

    private readonly IGitHubService gitHubService;

    public ProblemsController(ICorrelationIdService correlationIdService, IProblemService problemService, IGitHubService gitHubService)
        : base(correlationIdService)
    {
        this.problemService = problemService ?? throw new ArgumentNullException(nameof(problemService));
        this.gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));
    }

    /// <summary>
    /// Gets a paginated list of problems.
    /// </summary>
    /// <param name="searchParams">The query parameters for filtering and pagination.</param>
    /// <returns>A paginated list of problems.</returns>
    /// <response code="200">Returns the paginated list of problems.</response>
    [AllowAnonymous]
    [HttpGet(Name = nameof(GetProblemsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<ProblemDto>>> GetProblemsAsync([FromQuery] ProblemQueryParameters searchParams)
    {
        var problems = await this.problemService.GetProblemsAsync(searchParams, this.HttpContext.RequestAborted);
        var response = problems.ToCursorPaginatedResponse(searchParams);
        return this.Ok(response);
    }

    /// <summary>
    /// Gets a paginated list of problems with limited information.
    /// </summary>
    /// <param name="searchParams">The query parameters for filtering and pagination.</param>
    /// <returns>A paginated list of problems with limited details.</returns>
    /// <response code="200">Returns the paginated list of problems with limited information.</response>
    [AllowAnonymous]
    [HttpGet("limited", Name = nameof(GetProblemsLimitedAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<ProblemLimitedDto>>> GetProblemsLimitedAsync([FromQuery] ProblemQueryParameters searchParams)
    {
        var problems = await this.problemService.GetProblemsLimitedAsync(searchParams, this.HttpContext.RequestAborted);
        var response = problems.ToCursorPaginatedResponse(searchParams);
        return this.Ok(response);
    }

    /// <summary>
    /// Gets a personalized list of problems with limited information.
    /// </summary>
    /// <returns>A list of personalized problems with limited details.</returns>
    /// <response code="200">Returns the list of personalized problems with limited information.</response>
    [HttpGet("limited/personalized", Name = nameof(GetPersonalizedProblemListAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonalizedProblemLimitedDto>>> GetPersonalizedProblemListAsync()
    {
        var problems = await this.problemService.GetPersonalizedProblemListAsync(this.HttpContext.RequestAborted);
        return this.Ok(problems);
    }

    /// <summary>
    /// Gets the total count of problems.
    /// </summary>
    /// <returns>The total count of problems.</returns>
    /// <response code="200">Returns the total count of problems.</response>
    [AllowAnonymous]
    [HttpGet("count", Name = nameof(GetProblemsCountAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetProblemsCountAsync()
    {
        var count = await this.problemService.GetProblemsCountAsync(this.HttpContext.RequestAborted);
        return this.Ok(count);
    }

    /// <summary>
    /// Gets a specific problem by its ID.
    /// </summary>
    /// <param name="id">The ID of the problem to retrieve.</param>
    /// <returns>The problem with the specified ID.</returns>
    /// <response code="200">Returns the problem.</response>
    /// <response code="404">If the problem is not found.</response>
    [AllowAnonymous]
    [HttpGet("{id}", Name = nameof(GetProblemAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> GetProblemAsync([FromRoute] int id)
    {
        var problem = await this.problemService.GetProblemByIdAsync(id, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return this.NotFound($"Problem with ID {id} not found");
        }

        return this.Ok(problem);
    }

    /// <summary>
    /// Syncs problems from the database to GitHub. Admin access required.
    /// </summary>
    /// <returns>The URL of the created pull request.</returns>
    /// <response code="200">Returns the pull request URL.</response>
    [HttpPost("sync", Name = nameof(SyncProblemsToGitHubAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncProblemsToGitHubAsync()
    {
        var prUrl = await this.gitHubService.SyncProblemsFromDatabaseToGithubAsync(this.HttpContext.RequestAborted);
        return this.Ok(new { prUrl });
    }

    /// <summary>
    /// Creates a new problem. Admin access required.
    /// </summary>
    /// <param name="problem">The problem creation request.</param>
    /// <returns>The newly created problem.</returns>
    /// <response code="201">Returns the newly created problem.</response>
    [HttpPost(Name = nameof(CreateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProblemDto>> CreateProblemAsync([FromBody] CreateProblemRequest problem)
    {
        var newProblemResult = await this.problemService.CreateProblemAsync(problem, this.HttpContext.RequestAborted);

        if (!newProblemResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(newProblemResult);
        }

        var newProblem = newProblemResult.ValueOrThrow;

        return this.CreatedAtRoute(nameof(GetProblemAsync), new { id = newProblem.Id }, newProblem);
    }

    /// <summary>
    /// Clones an existing problem. Admin access required.
    /// </summary>
    /// <param name="problemId">The ID of the problem to clone.</param>
    /// <returns>The cloned problem.</returns>
    /// <response code="201">Returns the cloned problem.</response>
    /// <response code="404">If the problem to clone is not found.</response>
    [HttpPost("{problemId}/clone", Name = nameof(CloneProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> CloneProblemAsync([FromRoute] int problemId)
    {
        var cloneProblemResult = await this.problemService.CloneProblemAsync(problemId, this.HttpContext.RequestAborted);

        if (!cloneProblemResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(cloneProblemResult);
        }

        var clonedProblem = cloneProblemResult.ValueOrThrow;

        return this.CreatedAtRoute(nameof(GetProblemAsync), new { id = clonedProblem.Id }, clonedProblem);
    }

    /// <summary>
    /// Partially updates a problem using JSON Patch. Admin access required.
    /// </summary>
    /// <param name="problemId">The ID of the problem to update.</param>
    /// <param name="dtoPatchDoc">The JSON Patch document containing the updates.</param>
    /// <returns>The updated problem.</returns>
    /// <response code="200">Returns the updated problem.</response>
    /// <response code="404">If the problem is not found.</response>
    [HttpPatch("{problemId}", Name = nameof(UpdateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> UpdateProblemAsync(int problemId, [FromBody] JsonPatchDocument<CreateProblemRequest> dtoPatchDoc)
    {
        var patchedProblemResult = await this.problemService.PatchProblemAsync(problemId, dtoPatchDoc, this.HttpContext.RequestAborted);

        if (!patchedProblemResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(patchedProblemResult);
        }

        var patchedProblem = patchedProblemResult.ValueOrThrow;

        return this.Ok(patchedProblem);
    }

    /// <summary>
    /// Fully updates a problem. Admin access required.
    /// </summary>
    /// <param name="problemId">The ID of the problem to update.</param>
    /// <param name="updateRequest">The full problem update request.</param>
    /// <returns>The updated problem.</returns>
    /// <response code="200">Returns the updated problem.</response>
    /// <response code="404">If the problem is not found.</response>
    [HttpPut("{problemId}", Name = nameof(FullUpdateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> FullUpdateProblemAsync([FromRoute] int problemId, [FromBody] CreateProblemRequest updateRequest)
    {
        var updateProblemResult = await this.problemService.UpdateProblemAsync(problemId, updateRequest, this.HttpContext.RequestAborted);

        if (!updateProblemResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(updateProblemResult);
        }

        var updatedProblem = updateProblemResult.ValueOrThrow;

        return this.Ok(updatedProblem);
    }

    /// <summary>
    /// Deletes a problem. Admin access required.
    /// </summary>
    /// <param name="problemId">The ID of the problem to delete.</param>
    /// <param name="hardDelete">If true, permanently deletes the problem. Otherwise, soft deletes.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Problem was successfully deleted.</response>
    /// <response code="404">If the problem is not found.</response>
    [HttpDelete("{problemId}", Name = nameof(DeleteProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProblemAsync([FromRoute] int problemId, [FromQuery] bool hardDelete = false)
    {
        var deleteProblemResult = await this.problemService.DeleteProblemAsync(problemId, hardDelete, this.HttpContext.RequestAborted);

        if (!deleteProblemResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(deleteProblemResult);
        }

        return this.NoContent();
    }

    /// <summary>
    /// Validates a problem creation request without creating it. Admin access required.
    /// </summary>
    /// <param name="problem">The problem creation request to validate.</param>
    /// <returns>The validation response with any errors or warnings.</returns>
    /// <response code="200">Returns the validation result.</response>
    [HttpPost("validate", Name = nameof(ValidateCreateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateProblemValidationResponse>> ValidateCreateProblemAsync([FromBody] CreateProblemRequest problem)
    {
        var validationResponse = await this.problemService.ValidateCreateProblemAsync(problem, this.HttpContext.RequestAborted);
        return this.Ok(validationResponse);
    }
}