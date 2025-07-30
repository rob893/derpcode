using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
using DerpCode.API.Models.Responses;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Core;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using static DerpCode.API.Utilities.UtilityFunctions;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing problem-related business logic
/// </summary>
public sealed class ProblemService : IProblemService
{
    private readonly ILogger<ProblemService> logger;

    private readonly ICodeExecutionService codeExecutionService;

    private readonly IProblemRepository problemRepository;

    private readonly ICurrentUserService currentUserService;

    private readonly ITagRepository tagRepository;

    private readonly IMemoryCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemService"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="codeExecutionService">The code execution service</param>
    /// <param name="problemRepository">The problem repository</param>
    /// <param name="tagRepository">The tag repository</param>
    /// <param name="currentUserService">The current user service</param>
    /// <param name="cache">The memory cache</param>
    public ProblemService(
        ILogger<ProblemService> logger,
        ICodeExecutionService codeExecutionService,
        IProblemRepository problemRepository,
        ITagRepository tagRepository,
        ICurrentUserService currentUserService,
        IMemoryCache cache)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    public async Task<CursorPaginatedList<ProblemDto, int>> GetProblemsAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(searchParams);
        var problems = await this.GetProblemsFromCacheAsync(cancellationToken);

        var pagedList = problems.Values
            .Select(x => ProblemDto.FromEntity(x, this.currentUserService.IsAdmin, this.currentUserService.IsPremiumUser))
            .ToCursorPaginatedList(searchParams);

        return pagedList;
    }

    /// <inheritdoc />
    public async Task<ProblemDto?> GetProblemByIdAsync(int id, CancellationToken cancellationToken)
    {
        var problems = await this.GetProblemsFromCacheAsync(cancellationToken);

        if (!problems.TryGetValue(id, out var problem))
        {
            return null;
        }

        return ProblemDto.FromEntity(problem, this.currentUserService.IsAdmin, this.currentUserService.IsPremiumUser);
    }

    /// <inheritdoc />
    public async Task<Result<ProblemDto>> CreateProblemAsync(CreateProblemRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await this.CreateProblemAsync(request, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<ProblemDto>> CreateProblemAsync(CreateProblemRequest request, int? newProblemId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var newProblem = request.ToEntity();

        if (newProblemId.HasValue)
        {
            newProblem.Id = newProblemId.Value;
        }

        if (request.Tags != null && request.Tags.Count > 0)
        {
            var tagNames = request.Tags.Select(t => t.Name).ToHashSet();
            var existingTags = await this.tagRepository.SearchAsync(t => tagNames.Contains(t.Name), track: true, cancellationToken);
            newProblem.Tags = [.. existingTags.Union(newProblem.Tags.Where(t => !existingTags.Any(et => et.Name == t.Name)))];
        }

        newProblem.ExplanationArticle.UserId = this.currentUserService.UserId;
        newProblem.ExplanationArticle.LastEditedById = this.currentUserService.UserId;
        newProblem.ExplanationArticle.Tags = [.. newProblem.Tags];

        this.problemRepository.Add(newProblem);
        await this.problemRepository.SaveChangesAsync(cancellationToken);

        this.cache.Remove(CacheKeys.Problems);

        return Result<ProblemDto>.Success(ProblemDto.FromEntity(newProblem, this.currentUserService.IsAdmin, this.currentUserService.IsPremiumUser));
    }

    /// <inheritdoc />
    public async Task<Result<ProblemDto>> CloneProblemAsync(int existingProblemId, CancellationToken cancellationToken)
    {
        var existingProblem = await this.problemRepository.GetByIdAsync(existingProblemId, track: true, cancellationToken);

        if (existingProblem == null)
        {
            this.logger.LogWarning("Cannot clone problem {ProblemId}: Problem not found", existingProblemId);
            return Result<ProblemDto>.Failure(DomainErrorType.NotFound, $"Problem with ID {existingProblemId} not found.");
        }

        var cloneRequest = CreateProblemRequest.FromEntity(existingProblem) with { Name = $"{existingProblem.Name} (Clone)" };

        return await this.CreateProblemAsync(cloneRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<ProblemDto>> PatchProblemAsync(int problemId, JsonPatchDocument<CreateProblemRequest> patchDocument, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(patchDocument);

        if (patchDocument.Operations.Count == 0)
        {
            this.logger.LogWarning("Cannot patch problem {ProblemId}: No operations provided in patch document", problemId);
            return Result<ProblemDto>.Failure(DomainErrorType.Validation, "A JSON patch document with at least 1 operation is required.");
        }

        var problem = await this.problemRepository.GetByIdAsync(problemId, track: true, cancellationToken);

        if (problem == null)
        {
            this.logger.LogWarning("Cannot patch problem {ProblemId}: Problem not found", problemId);
            return Result<ProblemDto>.Failure(DomainErrorType.NotFound, $"Problem with ID {problemId} not found.");
        }

        var validationCheck = CreateProblemRequest.FromEntity(problem);
        if (!patchDocument.TryApply(validationCheck, out var validationError))
        {
            this.logger.LogWarning("Cannot patch problem {ProblemId}: Invalid patch document - {ValidationError}", problemId, validationError);
            return Result<ProblemDto>.Failure(DomainErrorType.Validation, $"Invalid JSON patch document: {validationError}");
        }

        var entityPatchDoc = patchDocument.MapPatchDocument<CreateProblemRequest, Problem>();

        if (!entityPatchDoc.TryApply(problem, out var error))
        {
            this.logger.LogError("Failed to apply patch document to problem {ProblemId} after successful validation: {Error}", problemId, error);
            return Result<ProblemDto>.Failure(DomainErrorType.Unknown, $"Failed to apply JSON patch document after successful validation: {error}");
        }

        var updated = await this.problemRepository.SaveChangesAsync(cancellationToken);

        if (updated == 0)
        {
            this.logger.LogError("Failed to patch problem {ProblemId}: No changes were saved", problemId);
            return Result<ProblemDto>.Failure(DomainErrorType.Unknown, "Failed to update problem. No changes were saved.");
        }

        this.cache.Remove(CacheKeys.Problems);

        return Result<ProblemDto>.Success(ProblemDto.FromEntity(problem, this.currentUserService.IsAdmin, this.currentUserService.IsPremiumUser));
    }

    /// <inheritdoc />
    public async Task<Result<ProblemDto>> UpdateProblemAsync(int problemId, CreateProblemRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingProblem = await this.problemRepository.GetByIdAsync(problemId, track: true, cancellationToken);

        if (existingProblem == null)
        {
            this.logger.LogWarning("Cannot update problem {ProblemId}: Problem not found", problemId);
            return Result<ProblemDto>.Failure(DomainErrorType.NotFound, $"Problem with ID {problemId} not found.");
        }

        var newProblem = request.ToEntity();

        if (request.Tags != null && request.Tags.Count > 0)
        {
            var tagNames = request.Tags.Select(t => t.Name).ToHashSet();
            var existingTags = await this.tagRepository.SearchAsync(t => tagNames.Contains(t.Name), track: true, cancellationToken);
            newProblem.Tags = [.. existingTags.Union(newProblem.Tags.Where(t => !existingTags.Any(et => et.Name == t.Name))).Where(x => tagNames.Contains(x.Name))];
            newProblem.ExplanationArticle.Tags = [.. existingTags.Union(newProblem.ExplanationArticle.Tags.Where(t => !existingTags.Any(et => et.Name == t.Name))).Where(x => tagNames.Contains(x.Name))];
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
        existingProblem.ExplanationArticle.Title = request.ExplanationArticle.Title;
        existingProblem.ExplanationArticle.Content = request.ExplanationArticle.Content;
        existingProblem.ExplanationArticle.UpdatedAt = DateTimeOffset.UtcNow;
        existingProblem.ExplanationArticle.LastEditedById = this.currentUserService.UserId;
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

        var updated = await this.problemRepository.SaveChangesAsync(cancellationToken);

        if (updated == 0)
        {
            this.logger.LogError("Failed to update problem {ProblemId}: No changes were saved", problemId);
            return Result<ProblemDto>.Failure(DomainErrorType.Unknown, "Failed to update problem. No changes were saved.");
        }

        this.cache.Remove(CacheKeys.Problems);

        return Result<ProblemDto>.Success(ProblemDto.FromEntity(existingProblem, this.currentUserService.IsAdmin, this.currentUserService.IsPremiumUser));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteProblemAsync(int problemId, CancellationToken cancellationToken)
    {
        var problem = await this.problemRepository.GetByIdAsync(problemId, [p => p.ExplanationArticle, p => p.SolutionArticles], track: true, cancellationToken);

        if (problem == null)
        {
            this.logger.LogWarning("Cannot delete problem {ProblemId}: Problem not found", problemId);
            return Result<bool>.Failure(DomainErrorType.NotFound, $"Problem with ID {problemId} not found.");
        }

        this.problemRepository.Remove(problem);

        var removed = await this.problemRepository.SaveChangesAsync(cancellationToken);

        if (removed == 0)
        {
            this.logger.LogError("Failed to delete problem {ProblemId}: No changes were saved", problemId);
            return Result<bool>.Failure(DomainErrorType.Unknown, "Failed to delete problem. No changes were saved.");
        }

        this.cache.Remove(CacheKeys.Problems);

        return Result<bool>.Success(true);
    }

    /// <inheritdoc />
    public async Task<CreateProblemValidationResponse> ValidateCreateProblemAsync(CreateProblemRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var newProblem = request.ToEntity();
        var driverValidations = new ConcurrentBag<CreateProblemDriverValidationResponse>();

        var tasks = newProblem.Drivers.Select(async driver =>
        {
            try
            {
                var result = await this.codeExecutionService.RunCodeAsync(this.currentUserService.UserId, driver.Answer, driver.Language, newProblem, cancellationToken);
                driverValidations.Add(new CreateProblemDriverValidationResponse
                {
                    Language = driver.Language,
                    IsValid = result.Pass,
                    Image = driver.Image,
                    ErrorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage) ?
                        result.Pass ? null : "Supplied driver answer did not pass supplied test cases." :
                        result.ErrorMessage,
                    SubmissionResult = ProblemSubmissionDto.FromEntity(result, this.currentUserService.IsAdmin || this.currentUserService.IsPremiumUser)
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
                        UserId = this.currentUserService.UserId,
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

        return response;
    }

    public static bool HasProblemChanged(Problem leftProblem, Problem rightProblem)
    {
        ArgumentNullException.ThrowIfNull(leftProblem);
        ArgumentNullException.ThrowIfNull(rightProblem);

        // Compare basic properties
        if (leftProblem.Name != rightProblem.Name ||
            leftProblem.Description != rightProblem.Description ||
            leftProblem.Difficulty != rightProblem.Difficulty ||
            leftProblem.ExplanationArticle.Content != rightProblem.ExplanationArticle.Content)
        {
            return true;
        }

        // Compare lists (Input, ExpectedOutput, Hints)
        if (!AreListsEqual(leftProblem.Input, rightProblem.Input) ||
            !AreListsEqual(leftProblem.ExpectedOutput, rightProblem.ExpectedOutput) ||
            !AreListsEqual(leftProblem.Hints, rightProblem.Hints))
        {
            return true;
        }

        // Compare tags
        if (!AreTagsEqual(leftProblem.Tags, rightProblem.Tags))
        {
            return true;
        }

        // Compare drivers
        if (!AreDriversEqual(leftProblem.Drivers, rightProblem.Drivers))
        {
            return true;
        }

        return false;
    }

    public static bool AreTagsEqual(List<Tag> leftTags, List<Tag> rightTags)
    {
        ArgumentNullException.ThrowIfNull(leftTags);
        ArgumentNullException.ThrowIfNull(rightTags);

        if (leftTags.Count != rightTags.Count)
        {
            return false;
        }

        var dbTagNames = leftTags.Select(t => t.Name).OrderBy(n => n).ToList();
        var folderTagNames = rightTags.Select(t => t.Name).OrderBy(n => n).ToList();

        return dbTagNames.SequenceEqual(folderTagNames);
    }

    public static bool AreDriversEqual(List<ProblemDriver> leftDrivers, List<ProblemDriver> rightDrivers)
    {
        ArgumentNullException.ThrowIfNull(leftDrivers);
        ArgumentNullException.ThrowIfNull(rightDrivers);

        if (leftDrivers.Count != rightDrivers.Count)
        {
            return false;
        }

        var dbDriversDict = leftDrivers.ToDictionary(d => d.Language);
        var folderDriversDict = rightDrivers.ToDictionary(d => d.Language);

        foreach (var kvp in folderDriversDict)
        {
            if (!dbDriversDict.TryGetValue(kvp.Key, out var dbDriver))
            {
                return false;
            }

            var folderDriver = kvp.Value;
            if (dbDriver.DriverCode != folderDriver.DriverCode ||
                dbDriver.UITemplate != folderDriver.UITemplate ||
                dbDriver.Answer != folderDriver.Answer ||
                dbDriver.Image != folderDriver.Image)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<IReadOnlyDictionary<int, Problem>> GetProblemsFromCacheAsync(CancellationToken cancellationToken)
    {
        if (!this.cache.TryGetValue(CacheKeys.Problems, out Dictionary<int, Problem>? problemLookup))
        {
            var problems = await this.problemRepository.SearchAsync(
                p => true,
                [p => p.ExplanationArticle, p => p.Tags, p => p.Drivers],
                track: false,
                cancellationToken);
            problemLookup = problems.ToDictionary(p => p.Id, p => p);

            this.cache.Set(CacheKeys.Problems, problemLookup, TimeSpan.FromDays(1));
        }

        return problemLookup ?? throw new InvalidOperationException("Failed to retrieve problems from cache.");
    }
}
