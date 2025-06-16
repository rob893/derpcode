using System;
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

    public ProblemsController(ICorrelationIdService correlationIdService, IProblemService problemService)
        : base(correlationIdService)
    {
        this.problemService = problemService ?? throw new ArgumentNullException(nameof(problemService));
    }

    [AllowAnonymous]
    [HttpGet(Name = nameof(GetProblemsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<ProblemDto>>> GetProblemsAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var problems = await this.problemService.GetProblemsAsync(searchParams, this.HttpContext.RequestAborted);
        var response = problems.ToCursorPaginatedResponse(searchParams);
        return this.Ok(response);
    }

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

    [HttpPatch("{problemId}", Name = nameof(UpdateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> UpdateProblemAsync(int problemId, [FromBody] JsonPatchDocument<CreateProblemRequest> dtoPatchDoc)
    {
        if (dtoPatchDoc == null || dtoPatchDoc.Operations.Count == 0)
        {
            return this.BadRequest("A JSON patch document with at least 1 operation is required.");
        }

        var patchedProblemResult = await this.problemService.PatchProblemAsync(problemId, dtoPatchDoc, this.HttpContext.RequestAborted);

        if (!patchedProblemResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(patchedProblemResult);
        }

        var patchedProblem = patchedProblemResult.ValueOrThrow;

        return this.Ok(patchedProblem);
    }

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

    [HttpDelete("{problemId}", Name = nameof(DeleteProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteProblemAsync([FromRoute] int problemId)
    {
        var deleteProblemResult = await this.problemService.DeleteProblemAsync(problemId, this.HttpContext.RequestAborted);

        if (!deleteProblemResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(deleteProblemResult);
        }

        return this.NoContent();
    }

    [HttpPost("validate", Name = nameof(ValidateCreateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateProblemValidationResponse>> ValidateCreateProblemAsync([FromBody] CreateProblemRequest problem)
    {
        var validationResponse = await this.problemService.ValidateCreateProblemAsync(problem, this.HttpContext.RequestAborted);
        return this.Ok(validationResponse);
    }
}