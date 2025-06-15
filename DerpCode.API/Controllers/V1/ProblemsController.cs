using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
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

    private readonly IArticleRepository articleRepository;

    private readonly ITagRepository tagRepository;

    public ProblemsController(
        ILogger<ProblemsController> logger,
        ICorrelationIdService correlationIdService,
        ICodeExecutionService codeExecutionService,
        IProblemRepository problemRepository,
        IArticleRepository articleRepository,
        ITagRepository tagRepository)
            : base(correlationIdService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
        this.articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
        this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
    }

    [AllowAnonymous]
    [HttpGet(Name = nameof(GetProblemsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<ProblemDto>>> GetProblemsAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var problems = await this.problemRepository.SearchAsync(searchParams, track: false, this.HttpContext.RequestAborted);
        var response = problems.Select(p => ProblemDto.FromEntity(p, this.User)).ToCursorPaginatedResponse(searchParams);
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

        return this.Ok(ProblemDto.FromEntity(problem, this.User));
    }

    [HttpPost(Name = nameof(CreateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProblemDto>> CreateProblemAsync([FromBody] CreateProblemRequest problem)
    {
        if (problem == null)
        {
            return this.BadRequest("Problem cannot be null");
        }

        if (!this.User.TryGetUserId(out var userId))
        {
            return this.Forbidden("You must be logged in to create a problem.");
        }

        var newProblem = problem.ToEntity();

        if (problem.Tags != null && problem.Tags.Count > 0)
        {
            var tagNames = problem.Tags.Select(t => t.Name).ToHashSet();
            var existingTags = await this.tagRepository.SearchAsync(t => tagNames.Contains(t.Name), track: true, this.HttpContext.RequestAborted);
            newProblem.Tags = [.. existingTags.Union(newProblem.Tags.Where(t => !existingTags.Any(et => et.Name == t.Name)))];

        }

        newProblem.ExplanationArticle.UserId = userId.Value;
        newProblem.ExplanationArticle.LastEditedById = userId.Value;
        newProblem.ExplanationArticle.Tags = [.. newProblem.Tags];


        this.problemRepository.Add(newProblem);
        await this.problemRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        return this.CreatedAtRoute(nameof(GetProblemAsync), new { id = newProblem.Id }, ProblemDto.FromEntity(newProblem, this.User));
    }

    [HttpPost("{problemId}/clone", Name = nameof(CloneProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProblemDto>> CloneProblemAsync([FromRoute] int problemId)
    {
        var existingProblem = await this.problemRepository.GetByIdAsync(problemId, [x => x.ExplanationArticle], this.HttpContext.RequestAborted);

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
    public async Task<ActionResult<ProblemDto>> UpdateProblemAsync(int problemId, [FromBody] JsonPatchDocument<CreateProblemRequest> dtoPatchDoc)
    {
        if (dtoPatchDoc == null || dtoPatchDoc.Operations.Count == 0)
        {
            return this.BadRequest("A JSON patch document with at least 1 operation is required.");
        }

        var problem = await this.problemRepository.GetByIdAsync(problemId, [x => x.ExplanationArticle], this.HttpContext.RequestAborted);

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

        var problemToReturn = ProblemDto.FromEntity(problem, this.User);

        return this.Ok(problemToReturn);
    }

    [HttpPut("{problemId}", Name = nameof(FullUpdateProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProblemDto>> FullUpdateProblemAsync([FromRoute] int problemId, [FromBody] CreateProblemRequest updateRequest)
    {
        if (updateRequest == null)
        {
            return this.BadRequest("Problem cannot be null");
        }

        if (!this.User.TryGetUserId(out var userId))
        {
            return this.Forbidden("You must be logged in to create a problem.");
        }

        var existingProblem = await this.problemRepository.GetByIdAsync(problemId, [x => x.ExplanationArticle], this.HttpContext.RequestAborted);

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
            newProblem.ExplanationArticle.Tags = [.. existingTags.Union(newProblem.ExplanationArticle.Tags.Where(t => !existingTags.Any(et => et.Name == t.Name)))];
        }

        // Update the existing problem with the new values
        existingProblem.Name = newProblem.Name;
        existingProblem.Description = newProblem.Description;
        existingProblem.Difficulty = newProblem.Difficulty;
        existingProblem.Tags = newProblem.Tags;
        existingProblem.Hints = newProblem.Hints;
        existingProblem.ExpectedOutput = newProblem.ExpectedOutput;
        existingProblem.Input = newProblem.Input;

        // Update the explanation article
        existingProblem.ExplanationArticle.Title = updateRequest.ExplanationArticle.Title;
        existingProblem.ExplanationArticle.Content = updateRequest.ExplanationArticle.Content;
        existingProblem.ExplanationArticle.UpdatedAt = DateTimeOffset.UtcNow;
        existingProblem.ExplanationArticle.LastEditedById = userId.Value;
        existingProblem.ExplanationArticle.Tags = newProblem.ExplanationArticle.Tags;

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

        return this.Ok(ProblemDto.FromEntity(existingProblem, this.User));
    }

    [HttpDelete("{problemId}", Name = nameof(DeleteProblemAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteProblemAsync([FromRoute] int problemId)
    {
        var problem = await this.problemRepository.GetByIdAsync(problemId, [p => p.ExplanationArticle, p => p.SolutionArticles], this.HttpContext.RequestAborted);
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

        if (!this.User.TryGetUserId(out var userId))
        {
            return this.Forbidden();
        }

        var newProblem = problem.ToEntity();

        var driverValidations = new ConcurrentBag<CreateProblemDriverValidationResponse>();

        var cancellationToken = this.HttpContext.RequestAborted;
        var tasks = newProblem.Drivers.Select(async driver =>
        {
            try
            {
                var result = await this.codeExecutionService.RunCodeAsync(userId.Value, driver.Answer, driver.Language, newProblem, cancellationToken);
                driverValidations.Add(new CreateProblemDriverValidationResponse
                {
                    Language = driver.Language,
                    IsValid = result.Pass,
                    Image = driver.Image,
                    ErrorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage) ?
                        result.Pass ? null : "Supplied driver answer did not pass supplied test cases." :
                        result.ErrorMessage,
                    SubmissionResult = ProblemSubmissionDto.FromEntity(result)
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
                    SubmissionResult = new ProblemSubmissionDto
                    {
                        Id = -1,
                        UserId = userId.Value,
                        ProblemId = newProblem.Id,
                        Language = driver.Language,
                        Code = driver.Answer,
                        CreatedAt = DateTimeOffset.UtcNow,
                        Pass = false,
                        TestCaseCount = -1,
                        PassedTestCases = -1,
                        FailedTestCases = -1,
                        ExecutionTimeInMs = -1,
                        ErrorMessage = ex.Message,
                        TestCaseResults = []
                    }
                });
            }
        });

        await Task.WhenAll(tasks);

        var isValid = driverValidations.All(dv => dv.IsValid);
        var response = new CreateProblemValidationResponse
        {
            IsValid = isValid,
            DriverValidations = driverValidations.ToList(),
            ErrorMessage = isValid ? null : "One or more driver templates failed validation"
        };

        return this.Ok(response);
    }
}