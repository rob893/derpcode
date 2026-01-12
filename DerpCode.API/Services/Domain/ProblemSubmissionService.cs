using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing problem submission-related business logic
/// </summary>
public sealed class ProblemSubmissionService : IProblemSubmissionService
{
    private readonly ILogger<ProblemSubmissionService> logger;

    private readonly ICodeExecutionService codeExecutionService;

    private readonly IProblemRepository problemRepository;

    private readonly IProblemSubmissionRepository problemSubmissionRepository;

    private readonly ICurrentUserService currentUserService;

    private readonly IMemoryCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemSubmissionService"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="codeExecutionService">The code execution service</param>
    /// <param name="problemRepository">The problem repository</param>
    /// <param name="problemSubmissionRepository">The problem submission repository</param>
    /// <param name="currentUserService">The current user service</param>
    /// <param name="cache">The memory cache</param>
    public ProblemSubmissionService(
        ILogger<ProblemSubmissionService> logger,
        ICodeExecutionService codeExecutionService,
        IProblemRepository problemRepository,
        IProblemSubmissionRepository problemSubmissionRepository,
        ICurrentUserService currentUserService,
        IMemoryCache cache)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
        this.problemSubmissionRepository = problemSubmissionRepository ?? throw new ArgumentNullException(nameof(problemSubmissionRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    public async Task<Result<ProblemSubmissionDto>> GetProblemSubmissionAsync(int problemId, int submissionId, CancellationToken cancellationToken)
    {
        var submission = await this.problemSubmissionRepository.GetByIdAsync(submissionId, track: false, cancellationToken);

        if (submission == null || submission.ProblemId != problemId)
        {
            this.logger.LogWarning("Submission {SubmissionId} for problem {ProblemId} not found or mismatch", submissionId, problemId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.NotFound, $"Submission with ID {submissionId} for problem with ID {problemId} not found");
        }

        if (!this.currentUserService.IsUserAuthorizedForResource(submission))
        {
            this.logger.LogWarning("User {UserId} attempted to access submission {SubmissionId} without authorization", this.currentUserService.UserId, submissionId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Forbidden, "You can only see your own submissions.");
        }

        return Result<ProblemSubmissionDto>.Success(ProblemSubmissionDto.FromEntity(submission, this.currentUserService.IsAdmin || this.currentUserService.IsPremiumUser, string.Empty));
    }

    /// <inheritdoc />
    public async Task<Result<ProblemSubmissionDto>> SubmitSolutionAsync(int problemId, ProblemSubmissionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(request.UserCode))
        {
            this.logger.LogWarning("User {UserId} attempted to submit empty code for problem {ProblemId}", this.currentUserService.UserId, problemId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Validation, "User code is required");
        }

        if (!this.currentUserService.EmailVerified)
        {
            this.logger.LogWarning("User {UserId} attempted to submit solution for problem {ProblemId} without verified email", this.currentUserService.UserId, problemId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Forbidden, "You must verify your email before submitting solutions.");
        }

        var problem = await this.problemRepository.GetByIdAsync(problemId, track: true, cancellationToken);

        if (problem == null)
        {
            this.logger.LogWarning("User {UserId} attempted to submit solution for non-existent problem {ProblemId}", this.currentUserService.UserId, problemId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.NotFound, $"Problem with ID {problemId} not found");
        }

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == request.Language);
        if (driver == null)
        {
            this.logger.LogWarning("User {UserId} attempted to submit solution for problem {ProblemId} with unsupported language {Language}", this.currentUserService.UserId, problemId, request.Language);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Validation, $"No driver template found for language {request.Language}");
        }

        try
        {
            var (result, stdOut) = await this.codeExecutionService.RunCodeAsync(this.currentUserService.UserId, request.UserCode, request.Language, problem, cancellationToken);

            problem.ProblemSubmissions.Add(result);
            var updated = await this.problemRepository.SaveChangesAsync(cancellationToken);

            if (updated == 0)
            {
                this.logger.LogError("Failed to save submission for problem {ProblemId} by user {UserId}", problemId, this.currentUserService.UserId);
                return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Unknown, "Failed to save submission. Please try again later.");
            }

            this.cache.Remove(CacheKeys.GetPersonalizedProblemsKey(this.currentUserService.UserId));

            return Result<ProblemSubmissionDto>.Success(ProblemSubmissionDto.FromEntity(result, this.currentUserService.IsAdmin || this.currentUserService.IsPremiumUser, stdOut));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code for problem {ProblemId} by user {UserId}", problemId, this.currentUserService.UserId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Unknown, "An error occurred while executing your code. Please try again later.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ProblemSubmissionDto>> RunSolutionAsync(int problemId, ProblemSubmissionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrEmpty(request.UserCode))
        {
            this.logger.LogWarning("User {UserId} attempted to run empty code for problem {ProblemId}", this.currentUserService.UserId, problemId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Validation, "User code is required");
        }

        var problem = await this.problemRepository.GetByIdAsync(problemId, track: false, cancellationToken);

        if (problem == null)
        {
            this.logger.LogWarning("User {UserId} attempted to run solution for non-existent problem {ProblemId}", this.currentUserService.UserId, problemId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.NotFound, $"Problem with ID {problemId} not found");
        }

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == request.Language);
        if (driver == null)
        {
            this.logger.LogWarning("User {UserId} attempted to run solution for problem {ProblemId} with unsupported language {Language}", this.currentUserService.UserId, problemId, request.Language);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Validation, $"No driver template found for language {request.Language}");
        }

        try
        {
            var (result, stdOut) = await this.codeExecutionService.RunCodeAsync(this.currentUserService.UserId, request.UserCode, request.Language, problem, cancellationToken);
            return Result<ProblemSubmissionDto>.Success(ProblemSubmissionDto.FromEntity(result, this.currentUserService.IsAdmin || this.currentUserService.IsPremiumUser, stdOut));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code for problem {ProblemId} by user {UserId}", problemId, this.currentUserService.UserId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Unknown, "An error occurred while executing your code. Please try again later.");
        }
    }
}
