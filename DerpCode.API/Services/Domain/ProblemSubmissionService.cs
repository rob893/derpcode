using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Core;
using DerpCode.API.Utilities;
using Microsoft.EntityFrameworkCore;
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

    private readonly IUserProblemProgressRepository userProblemProgressRepository;

    private readonly IUserProgressRepository userProgressRepository;

    private readonly IExperienceEventRepository experienceEventRepository;

    private readonly ICurrentUserService currentUserService;

    private readonly IMemoryCache cache;

    private readonly IProgressionService progressionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemSubmissionService"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="codeExecutionService">The code execution service</param>
    /// <param name="problemRepository">The problem repository</param>
    /// <param name="problemSubmissionRepository">The problem submission repository</param>
    /// <param name="userProblemProgressRepository">The user-problem progress repository</param>
    /// <param name="userProgressRepository">The user progress repository</param>
    /// <param name="experienceEventRepository">The experience event repository</param>
    /// <param name="currentUserService">The current user service</param>
    /// <param name="cache">The memory cache</param>
    /// <param name="progressionService">The progression service</param>
    public ProblemSubmissionService(
        ILogger<ProblemSubmissionService> logger,
        ICodeExecutionService codeExecutionService,
        IProblemRepository problemRepository,
        IProblemSubmissionRepository problemSubmissionRepository,
        IUserProblemProgressRepository userProblemProgressRepository,
        IUserProgressRepository userProgressRepository,
        IExperienceEventRepository experienceEventRepository,
        ICurrentUserService currentUserService,
        IMemoryCache cache,
        IProgressionService progressionService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
        this.problemSubmissionRepository = problemSubmissionRepository ?? throw new ArgumentNullException(nameof(problemSubmissionRepository));
        this.userProblemProgressRepository = userProblemProgressRepository ?? throw new ArgumentNullException(nameof(userProblemProgressRepository));
        this.userProgressRepository = userProgressRepository ?? throw new ArgumentNullException(nameof(userProgressRepository));
        this.experienceEventRepository = experienceEventRepository ?? throw new ArgumentNullException(nameof(experienceEventRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.progressionService = progressionService ?? throw new ArgumentNullException(nameof(progressionService));
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
    public async Task<Result<bool>> OpenHintAsync(int problemId, int hintIndex, CancellationToken cancellationToken)
    {
        if (hintIndex < 0)
        {
            return Result<bool>.Failure(DomainErrorType.Validation, "Hint index cannot be negative.");
        }

        var problem = await this.problemRepository.GetByIdAsync(problemId, track: false, cancellationToken);
        if (problem == null)
        {
            return Result<bool>.Failure(DomainErrorType.NotFound, $"Problem with ID {problemId} not found");
        }

        if (hintIndex >= problem.Hints.Count)
        {
            return Result<bool>.Failure(DomainErrorType.Validation, $"Hint index {hintIndex} is out of range for this problem.");
        }

        var userId = this.currentUserService.UserId;
        var now = DateTimeOffset.UtcNow;
        var progress = await this.userProblemProgressRepository.GetByUserAndProblemAsync(userId, problemId, track: true, cancellationToken)
            ?? this.CreateProgress(userId, problemId);

        this.EnsureCycleTrackingInitialized(progress, now);
        if (!progress.OpenedHintIndicesCurrentCycle.Contains(hintIndex))
        {
            progress.OpenedHintIndicesCurrentCycle = [.. progress.OpenedHintIndicesCurrentCycle, hintIndex];
        }

        await this.userProblemProgressRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
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

        var userId = this.currentUserService.UserId;
        var userProgress = await this.userProgressRepository.GetByUserIdAsync(userId, track: true, cancellationToken)
            ?? this.CreateUserProgress(userId);

        var problem = await this.problemRepository.GetByIdAsync(problemId, track: true, cancellationToken);
        if (problem == null)
        {
            this.logger.LogWarning("User {UserId} attempted to submit solution for non-existent problem {ProblemId}", userId, problemId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.NotFound, $"Problem with ID {problemId} not found");
        }

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == request.Language);
        if (driver == null)
        {
            this.logger.LogWarning("User {UserId} attempted to submit solution for problem {ProblemId} with unsupported language {Language}", userId, problemId, request.Language);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Validation, $"No driver template found for language {request.Language}");
        }

        var progress = await this.userProblemProgressRepository.GetByUserAndProblemAsync(userId, problemId, track: true, cancellationToken)
            ?? this.CreateProgress(userId, problemId);

        var submitStartedAt = DateTimeOffset.UtcNow;
        this.EnsureCycleTrackingInitialized(progress, submitStartedAt);
        progress.SubmitAttemptsCurrentCycle++;

        try
        {
            var (submissionEntity, stdOut) = await this.codeExecutionService.RunCodeAsync(userId, request.UserCode, request.Language, problem, cancellationToken);

            var xpResult = this.AwardXp(submissionEntity, problem, progress, userProgress);

            problem.ProblemSubmissions.Add(submissionEntity);

            try
            {
                var updated = await this.problemRepository.SaveChangesAsync(cancellationToken);
                if (updated == 0)
                {
                    this.logger.LogError("Failed to save submission for problem {ProblemId} by user {UserId}", problemId, userId);
                    return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Unknown, "Failed to save submission. Please try again later.");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                this.logger.LogWarning(ex, "Concurrency conflict saving XP for user {UserId} on problem {ProblemId}. Submission saved but XP may need retry.", userId, problemId);
                return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Conflict, "Your submission was processed but XP could not be updated due to a concurrent change. Please refresh and try again.");
            }

            this.cache.Remove(CacheKeys.GetPersonalizedProblemsKey(userId));

            var dto = ProblemSubmissionDto.FromEntity(
                submissionEntity,
                this.currentUserService.IsAdmin || this.currentUserService.IsPremiumUser,
                stdOut,
                xpResult);

            return Result<ProblemSubmissionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code for problem {ProblemId} by user {UserId}", problemId, userId);
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

            var userProgress = await this.userProgressRepository.GetByUserIdAsync(this.currentUserService.UserId, track: false, cancellationToken);
            var totalXp = userProgress?.TotalXp ?? 0;
            var levelProgress = this.progressionService.GetLevelProgress(totalXp);

            var xpResult = new XpResult
            {
                XpEarnedThisSubmission = 0,
                XpDeltaApplied = 0,
                ProblemBestXp = 0,
                TotalXp = totalXp,
                Level = levelProgress.Level,
                XpIntoLevel = levelProgress.XpIntoLevel,
                XpForNextLevel = levelProgress.XpForNextLevel,
                NextEligibleAt = null,
                IsXpEligibleThisSubmission = false
            };

            return Result<ProblemSubmissionDto>.Success(ProblemSubmissionDto.FromEntity(
                result,
                this.currentUserService.IsAdmin || this.currentUserService.IsPremiumUser,
                stdOut,
                xpResult));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code for problem {ProblemId} by user {UserId}", problemId, this.currentUserService.UserId);
            return Result<ProblemSubmissionDto>.Failure(DomainErrorType.Unknown, "An error occurred while executing your code. Please try again later.");
        }
    }

    private XpResult AwardXp(ProblemSubmission submissionEntity, Problem problem, UserProblemProgress progress, UserProgress userProgress)
    {
        if (!submissionEntity.Pass)
        {
            return this.BuildXpResultForNonPass(progress, userProgress, submissionEntity.CreatedAt);
        }

        var (isEligibleForAward, cycleIndex) = this.GetAwardEligibility(progress, submissionEntity.CreatedAt);

        if (!isEligibleForAward)
        {
            return this.BuildXpResultForCooldown(progress, userProgress, submissionEntity);
        }

        this.EnsureCycleTrackingInitialized(progress, submissionEntity.CreatedAt);

        var attemptsBeforePass = Math.Max(progress.SubmitAttemptsCurrentCycle - 1, 0);
        var firstSubmitInCycle = progress.FirstSubmitAtCurrentCycle ?? submissionEntity.CreatedAt;
        var solveDuration = submissionEntity.CreatedAt - firstSubmitInCycle;
        var hintsOpened = progress.OpenedHintIndicesCurrentCycle.Count;

        var xpEarned = this.progressionService.CalculateEarnedXp(problem.Difficulty, attemptsBeforePass, solveDuration, hintsOpened);

        if (progress.FirstXpAwardedAt == null)
        {
            progress.FirstXpAwardedAt = submissionEntity.CreatedAt;
            cycleIndex = 0;
        }

        progress.LastAwardedCycleIndex = cycleIndex;
        progress.LastSolvedAt = submissionEntity.CreatedAt;

        var xpDelta = 0;
        if (xpEarned > progress.BestXp)
        {
            xpDelta = xpEarned - progress.BestXp;
            progress.BestXp = xpEarned;
        }

        userProgress.TotalXp += xpDelta;
        var levelProgressAfter = this.progressionService.GetLevelProgress(userProgress.TotalXp);
        userProgress.Level = levelProgressAfter.Level;
        userProgress.UpdatedAt = DateTimeOffset.UtcNow;

        var metadata = JsonSerializer.Serialize(new
        {
            ProblemId = problem.Id,
            Difficulty = problem.Difficulty.ToString(),
            AttemptsBeforePass = attemptsBeforePass,
            HintsOpened = hintsOpened,
            SolveDurationSeconds = Math.Max((int)solveDuration.TotalSeconds, 0),
            EarnedXp = xpEarned,
            BestXp = progress.BestXp,
            DeltaApplied = xpDelta,
            CycleIndex = cycleIndex,
            Eligible = true
        });

        this.experienceEventRepository.Add(new ExperienceEvent
        {
            UserId = userProgress.UserId,
            EventType = ExperienceEventType.ProblemSolved,
            SourceType = ExperienceEventSourceType.Problem,
            SourceId = problem.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            XpDelta = xpDelta,
            IdempotencyKey = $"user:{userProgress.UserId}:problem:{problem.Id}:cycle:{cycleIndex}:best:{xpEarned}",
            Metadata = metadata,
            CreatedAt = submissionEntity.CreatedAt
        });

        ResetCycleTracking(progress);

        DateTimeOffset? nextEligibleAt = progress.FirstXpAwardedAt != null
            ? ProgressionMath.GetNextCycleStart(progress.FirstXpAwardedAt.Value, cycleIndex)
            : null;

        return new XpResult
        {
            XpEarnedThisSubmission = xpEarned,
            XpDeltaApplied = xpDelta,
            ProblemBestXp = progress.BestXp,
            TotalXp = userProgress.TotalXp,
            Level = levelProgressAfter.Level,
            XpIntoLevel = levelProgressAfter.XpIntoLevel,
            XpForNextLevel = levelProgressAfter.XpForNextLevel,
            NextEligibleAt = nextEligibleAt,
            IsXpEligibleThisSubmission = true
        };
    }

    private XpResult BuildXpResultForNonPass(UserProblemProgress progress, UserProgress userProgress, DateTimeOffset submittedAt)
    {
        DateTimeOffset? nextEligibleAt = null;
        if (progress.FirstXpAwardedAt != null)
        {
            var currentCycle = this.progressionService.GetCycleIndex(progress.FirstXpAwardedAt.Value, submittedAt);
            if (currentCycle <= progress.LastAwardedCycleIndex)
            {
                nextEligibleAt = ProgressionMath.GetNextCycleStart(progress.FirstXpAwardedAt.Value, progress.LastAwardedCycleIndex);
            }
        }

        var levelProgress = this.progressionService.GetLevelProgress(userProgress.TotalXp);

        return new XpResult
        {
            XpEarnedThisSubmission = 0,
            XpDeltaApplied = 0,
            ProblemBestXp = progress.BestXp,
            TotalXp = userProgress.TotalXp,
            Level = levelProgress.Level,
            XpIntoLevel = levelProgress.XpIntoLevel,
            XpForNextLevel = levelProgress.XpForNextLevel,
            NextEligibleAt = nextEligibleAt,
            IsXpEligibleThisSubmission = false
        };
    }

    private XpResult BuildXpResultForCooldown(UserProblemProgress progress, UserProgress userProgress, ProblemSubmission submissionEntity)
    {
        DateTimeOffset? nextEligibleAt = null;
        if (progress.FirstXpAwardedAt != null)
        {
            this.experienceEventRepository.Add(new ExperienceEvent
            {
                UserId = userProgress.UserId,
                EventType = ExperienceEventType.ProblemSolved,
                SourceType = ExperienceEventSourceType.Problem,
                SourceId = submissionEntity.ProblemId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                XpDelta = 0,
                IdempotencyKey = $"user:{userProgress.UserId}:problem:{submissionEntity.ProblemId}:cooldown:{submissionEntity.CreatedAt.ToUnixTimeMilliseconds()}",
                Metadata = JsonSerializer.Serialize(new
                {
                    ProblemId = submissionEntity.ProblemId,
                    Eligible = false,
                    Reason = "CooldownNotMet"
                }),
                CreatedAt = submissionEntity.CreatedAt
            });

            nextEligibleAt = ProgressionMath.GetNextCycleStart(progress.FirstXpAwardedAt.Value, progress.LastAwardedCycleIndex);
        }

        var levelProgress = this.progressionService.GetLevelProgress(userProgress.TotalXp);

        return new XpResult
        {
            XpEarnedThisSubmission = 0,
            XpDeltaApplied = 0,
            ProblemBestXp = progress.BestXp,
            TotalXp = userProgress.TotalXp,
            Level = levelProgress.Level,
            XpIntoLevel = levelProgress.XpIntoLevel,
            XpForNextLevel = levelProgress.XpForNextLevel,
            NextEligibleAt = nextEligibleAt,
            IsXpEligibleThisSubmission = false
        };
    }

    private UserProblemProgress CreateProgress(int userId, int problemId)
    {
        var progress = new UserProblemProgress
        {
            UserId = userId,
            ProblemId = problemId
        };

        this.userProblemProgressRepository.Add(progress);

        return progress;
    }

    private UserProgress CreateUserProgress(int userId)
    {
        var progress = new UserProgress
        {
            UserId = userId,
            TotalXp = 0,
            Level = 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        this.userProgressRepository.Add(progress);

        return progress;
    }

    private (bool IsEligible, int CycleIndex) GetAwardEligibility(UserProblemProgress progress, DateTimeOffset at)
    {
        if (progress.FirstXpAwardedAt == null)
        {
            return (true, 0);
        }

        var cycleIndex = this.progressionService.GetCycleIndex(progress.FirstXpAwardedAt.Value, at);
        return (cycleIndex > progress.LastAwardedCycleIndex, cycleIndex);
    }

    private void EnsureCycleTrackingInitialized(UserProblemProgress progress, DateTimeOffset at)
    {
        if (progress.FirstXpAwardedAt == null)
        {
            if (progress.FirstSubmitAtCurrentCycle == null)
            {
                progress.FirstSubmitAtCurrentCycle = at;
                progress.SubmitAttemptsCurrentCycle = 0;
                progress.OpenedHintIndicesCurrentCycle = [];
            }

            return;
        }

        var currentCycleIndex = this.progressionService.GetCycleIndex(progress.FirstXpAwardedAt.Value, at);
        var currentCycleStart = progress.FirstXpAwardedAt.Value.AddMonths(currentCycleIndex);

        if (progress.FirstSubmitAtCurrentCycle == null || progress.FirstSubmitAtCurrentCycle.Value < currentCycleStart)
        {
            progress.FirstSubmitAtCurrentCycle = at;
            progress.SubmitAttemptsCurrentCycle = 0;
            progress.OpenedHintIndicesCurrentCycle = [];
        }
    }

    private static void ResetCycleTracking(UserProblemProgress progress)
    {
        progress.FirstSubmitAtCurrentCycle = null;
        progress.SubmitAttemptsCurrentCycle = 0;
        progress.OpenedHintIndicesCurrentCycle = [];
    }
}
