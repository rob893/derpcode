using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
using DerpCode.API.Models.Responses;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/problems")]
[ApiVersion("1.0")]
public sealed class ProblemsController : ServiceControllerBase
{
    private readonly ILogger<ProblemsController> logger;

    private readonly ICodeExecutionService codeExecutionService;

    private readonly IProblemRepository problemRepository;

    private readonly ITagRepository tagRepository;

    public ProblemsController(
        ILogger<ProblemsController> logger,
        ICorrelationIdService correlationIdService,
        ICodeExecutionService codeExecutionService,
        IProblemRepository problemRepository,
        ITagRepository tagRepository)
            : base(correlationIdService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
        this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
    }

    [AllowAnonymous]
    [HttpGet(Name = nameof(GetProblemsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<ProblemDto>>> GetProblemsAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var problems = await this.problemRepository.SearchAsync(searchParams, track: false, this.HttpContext.RequestAborted);
        var response = problems.Select(ProblemDto.FromEntity).ToCursorPaginatedResponse(searchParams);
        return this.Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{id}", Name = nameof(GetProblemAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> GetProblemAsync([FromRoute] int id)
    {
        var problem = await this.problemRepository.GetByIdAsync(id, track: false, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return this.NotFound($"Problem with ID {id} not found");
        }

        return this.Ok(ProblemDto.FromEntity(problem));
    }

    [HttpGet("admin/{id}", Name = nameof(GetAdminProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProblemDto>> GetAdminProblemAsync([FromRoute] int id)
    {
        var problem = await this.problemRepository.GetByIdAsync(id, track: false, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return this.NotFound($"Problem with ID {id} not found");
        }

        return this.Ok(AdminProblemDto.FromEntity(problem));
    }

    [HttpPost(Name = nameof(CreateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<AdminProblemDto>> CreateProblemAsync([FromBody] CreateProblemRequest problem)
    {
        if (problem == null)
        {
            return this.BadRequest("Problem cannot be null");
        }

        var newProblem = problem.ToEntity();

        if (problem.Tags != null && problem.Tags.Count > 0)
        {
            var tagNames = problem.Tags.Select(t => t.Name).ToHashSet();
            var existingTags = await this.tagRepository.SearchAsync(t => tagNames.Contains(t.Name), track: true, this.HttpContext.RequestAborted);
            newProblem.Tags = [.. existingTags.Union(newProblem.Tags.Where(t => !existingTags.Any(et => et.Name == t.Name)))];
        }

        this.problemRepository.Add(newProblem);
        await this.problemRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        return this.CreatedAtRoute(nameof(GetProblemAsync), new { id = newProblem.Id }, AdminProblemDto.FromEntity(newProblem));
    }

    [HttpPost("{problemId}/clone", Name = nameof(CloneProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<AdminProblemDto>> CloneProblemAsync([FromRoute] int problemId)
    {
        var existingProblem = await this.problemRepository.GetByIdAsync(problemId, track: false, this.HttpContext.RequestAborted);

        if (existingProblem == null)
        {
            return this.NotFound($"Problem with ID {problemId} not found");
        }

        var problem = CreateProblemRequest.FromEntity(existingProblem) with { Name = $"{existingProblem.Name} (Clone)" };

        return await CreateProblemAsync(problem);
    }

    [HttpPatch("{problemId}", Name = nameof(UpdateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminProblemDto>> UpdateProblemAsync(int problemId, [FromBody] JsonPatchDocument<CreateProblemRequest> dtoPatchDoc)
    {
        if (dtoPatchDoc == null || dtoPatchDoc.Operations.Count == 0)
        {
            return this.BadRequest("A JSON patch document with at least 1 operation is required.");
        }

        var problem = await this.problemRepository.GetByIdAsync(problemId, track: true, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return this.NotFound($"No problem with Id {problemId} found.");
        }

        var validationCheck = CreateProblemRequest.FromEntity(problem);

        if (!dtoPatchDoc.TryApply(validationCheck, out var validationError))
        {
            return this.BadRequest($"Invalid JSON patch document: {validationError}");
        }

        var patchDoc = dtoPatchDoc.MapPatchDocument<CreateProblemRequest, Problem>();

        if (!patchDoc.TryApply(problem, out var error))
        {
            return this.BadRequest($"Invalid JSON patch document: {error}");
        }

        var updated = await this.problemRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        if (updated == 0)
        {
            return this.InternalServerError("Failed to update problem. Please try again later.");
        }

        var problemToReturn = AdminProblemDto.FromEntity(problem);

        return this.Ok(problemToReturn);
    }

    [HttpPut("{problemId}", Name = nameof(FullUpdateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminProblemDto>> FullUpdateProblemAsync([FromRoute] int problemId, [FromBody] CreateProblemRequest updateRequest)
    {
        if (updateRequest == null)
        {
            return this.BadRequest("Problem cannot be null");
        }

        var existingProblem = await this.problemRepository.GetByIdAsync(problemId, track: true, this.HttpContext.RequestAborted);

        if (existingProblem == null)
        {
            return this.NotFound($"Problem with ID {problemId} not found");
        }

        var newProblem = updateRequest.ToEntity();

        if (updateRequest.Tags != null && updateRequest.Tags.Count > 0)
        {
            var tagNames = updateRequest.Tags.Select(t => t.Name).ToHashSet();
            var existingTags = await this.tagRepository.SearchAsync(t => tagNames.Contains(t.Name), track: true, this.HttpContext.RequestAborted);
            newProblem.Tags = [.. existingTags.Union(newProblem.Tags.Where(t => !existingTags.Any(et => et.Name == t.Name)))];
        }

        // Update the existing problem with the new values
        existingProblem.Name = newProblem.Name;
        existingProblem.Description = newProblem.Description;
        existingProblem.Difficulty = newProblem.Difficulty;
        existingProblem.Tags = newProblem.Tags;
        existingProblem.Hints = newProblem.Hints;
        existingProblem.ExpectedOutput = newProblem.ExpectedOutput;
        existingProblem.Input = newProblem.Input;

        var newProblemDrivers = newProblem.Drivers.Select(d => d.Language).ToHashSet();
        existingProblem.Drivers.RemoveAll(x => !newProblemDrivers.Contains(x.Language));

        foreach (var driver in newProblem.Drivers)
        {
            var existingDriver = existingProblem.Drivers.FirstOrDefault(d => d.Language == driver.Language);

            if (existingDriver != null)
            {
                // Update existing driver
                existingDriver.Answer = driver.Answer;
                existingDriver.Image = driver.Image;
                existingDriver.DriverCode = driver.DriverCode;
                existingDriver.UITemplate = driver.UITemplate;
            }
            else
            {
                // Add new driver
                existingProblem.Drivers.Add(driver);
            }
        }

        var updated = await this.problemRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        if (updated == 0)
        {
            return this.InternalServerError("Failed to update problem. Please try again later.");
        }

        return this.Ok(AdminProblemDto.FromEntity(existingProblem));
    }

    [HttpDelete("{problemId}", Name = nameof(DeleteProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteProblemAsync([FromRoute] int problemId)
    {
        var problem = await this.problemRepository.GetByIdAsync(problemId, track: true, this.HttpContext.RequestAborted);
        if (problem == null)
        {
            return this.NotFound($"Problem with ID {problemId} not found");
        }

        this.problemRepository.Remove(problem);

        var removed = await this.problemRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        if (removed == 0)
        {
            return this.InternalServerError("Failed to delete problem. Please try again later.");
        }

        return this.NoContent();
    }

    [HttpPost("validate", Name = nameof(ValidateCreateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateProblemValidationResponse>> ValidateCreateProblemAsync([FromBody] CreateProblemRequest problem)
    {
        if (problem == null)
        {
            return this.BadRequest("Problem cannot be null");
        }

        var newProblem = problem.ToEntity();

        var driverValidations = new List<CreateProblemDriverValidationResponse>();
        foreach (var driver in newProblem.Drivers)
        {
            try
            {
                var result = await this.codeExecutionService.RunCodeAsync(driver.Answer, driver.Language, newProblem, this.HttpContext.RequestAborted);
                driverValidations.Add(new CreateProblemDriverValidationResponse
                {
                    Language = driver.Language,
                    IsValid = result.Pass,
                    Image = driver.Image,
                    ErrorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage) ?
                        result.Pass ? null : "Supplied driver answer did not pass supplied test cases." :
                        result.ErrorMessage,
                    SubmissionResult = result
                });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error validating driver template for language {Language}", driver.Language);
                driverValidations.Add(new CreateProblemDriverValidationResponse
                {
                    Language = driver.Language,
                    IsValid = false,
                    ErrorMessage = ex.Message,
                    Image = driver.Image,
                    SubmissionResult = new SubmissionResult
                    {
                        Pass = false,
                        ErrorMessage = ex.Message
                    }
                });
            }
        }

        var isValid = driverValidations.All(dv => dv.IsValid);
        var response = new CreateProblemValidationResponse
        {
            IsValid = isValid,
            DriverValidations = driverValidations,
            ErrorMessage = isValid ? null : "One or more driver templates failed validation"
        };

        return this.Ok(response);
    }
}