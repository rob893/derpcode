using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using DerpCode.API.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Tests.Services.Domain;

/// <summary>
/// Unit tests for the <see cref="ProblemSubmissionService"/> class
/// </summary>
public sealed class ProblemSubmissionServiceTests
{
    private readonly Mock<ILogger<ProblemSubmissionService>> mockLogger;

    private readonly Mock<ICodeExecutionService> mockCodeExecutionService;

    private readonly Mock<IProblemRepository> mockProblemRepository;

    private readonly Mock<IProblemSubmissionRepository> mockProblemSubmissionRepository;

    private readonly Mock<ICurrentUserService> mockCurrentUserService;

    private readonly Mock<IUserProblemProgressRepository> mockUserProblemProgressRepository;

    private readonly Mock<IUserProgressRepository> mockUserProgressRepository;

    private readonly Mock<IExperienceEventRepository> mockExperienceEventRepository;

    private readonly Mock<IProgressionService> mockProgressionService;

    private readonly Mock<IMemoryCache> mockCache;

    private readonly ProblemSubmissionService problemSubmissionService;

    public ProblemSubmissionServiceTests()
    {
        this.mockLogger = new Mock<ILogger<ProblemSubmissionService>>();
        this.mockCodeExecutionService = new Mock<ICodeExecutionService>();
        this.mockProblemRepository = new Mock<IProblemRepository>();
        this.mockProblemSubmissionRepository = new Mock<IProblemSubmissionRepository>();
        this.mockCurrentUserService = new Mock<ICurrentUserService>();
        this.mockUserProblemProgressRepository = new Mock<IUserProblemProgressRepository>();
        this.mockUserProgressRepository = new Mock<IUserProgressRepository>();
        this.mockExperienceEventRepository = new Mock<IExperienceEventRepository>();
        this.mockProgressionService = new Mock<IProgressionService>();
        this.mockCache = new Mock<IMemoryCache>();

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.EmailVerified).Returns(true);
        this.mockUserProgressRepository
            .Setup(x => x.GetByUserIdAsync(1, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress { UserId = 1, Level = 1, TotalXp = 0 });
        this.mockProgressionService
            .Setup(x => x.GetLevelProgress(It.IsAny<int>()))
            .Returns(new LevelProgress(1, 0, 100));
        this.mockProgressionService
            .Setup(x => x.CalculateEarnedXp(It.IsAny<ProblemDifficulty>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
            .Returns(50);

        this.problemSubmissionService = new ProblemSubmissionService(
            this.mockLogger.Object,
            this.mockCodeExecutionService.Object,
            this.mockProblemRepository.Object,
            this.mockProblemSubmissionRepository.Object,
            this.mockUserProblemProgressRepository.Object,
            this.mockUserProgressRepository.Object,
            this.mockExperienceEventRepository.Object,
            this.mockCurrentUserService.Object,
            this.mockCache.Object,
            this.mockProgressionService.Object);
    }

    #region GetProblemSubmissionAsync Tests

    [Fact]
    public async Task GetProblemSubmissionAsync_WithNonExistentSubmission_ReturnsNotFoundResult()
    {
        // Arrange
        var problemId = 1;
        var submissionId = 1;

        this.mockProblemSubmissionRepository
            .Setup(x => x.GetByIdAsync(submissionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProblemSubmission?)null);

        // Act
        var result = await this.problemSubmissionService.GetProblemSubmissionAsync(problemId, submissionId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetProblemSubmissionAsync_WithMismatchedProblemId_ReturnsNotFoundResult()
    {
        // Arrange
        var problemId = 1;
        var submissionId = 1;
        var submission = new ProblemSubmission
        {
            Id = submissionId,
            ProblemId = 2, // Different problem ID
            UserId = 1
        };

        this.mockProblemSubmissionRepository
            .Setup(x => x.GetByIdAsync(submissionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        // Act
        var result = await this.problemSubmissionService.GetProblemSubmissionAsync(problemId, submissionId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetProblemSubmissionAsync_WithUnauthorizedUser_ReturnsForbiddenResult()
    {
        // Arrange
        var problemId = 1;
        var submissionId = 1;
        var submission = new ProblemSubmission
        {
            Id = submissionId,
            ProblemId = problemId,
            UserId = 2 // Different user ID
        };

        this.mockProblemSubmissionRepository
            .Setup(x => x.GetByIdAsync(submissionId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        this.mockCurrentUserService
            .Setup(x => x.IsUserAuthorizedForResource(submission, true))
            .Returns(false);

        // Act
        var result = await this.problemSubmissionService.GetProblemSubmissionAsync(problemId, submissionId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Contains("own submissions", result.ErrorMessage);
    }

    #endregion

    #region SubmitSolutionAsync Tests

    [Fact]
    public async Task SubmitSolutionAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var problemId = 1;
        ProblemSubmissionRequest? request = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.problemSubmissionService.SubmitSolutionAsync(problemId, request!, CancellationToken.None));
    }

    [Fact]
    public async Task SubmitSolutionAsync_WithNullUserCode_ReturnsValidationError()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = null!,
            Language = LanguageType.CSharp
        };

        // Act
        var result = await this.problemSubmissionService.SubmitSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("User code is required", result.ErrorMessage);
    }

    [Fact]
    public async Task SubmitSolutionAsync_WithEmptyUserCode_ReturnsValidationError()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = "",
            Language = LanguageType.CSharp
        };

        // Act
        var result = await this.problemSubmissionService.SubmitSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("User code is required", result.ErrorMessage);
    }

    [Fact]
    public async Task SubmitSolutionAsync_WithUnverifiedEmail_ReturnsForbiddenResult()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = "console.log('test');",
            Language = LanguageType.CSharp
        };

        this.mockCurrentUserService.Setup(x => x.EmailVerified).Returns(false);

        // Act
        var result = await this.problemSubmissionService.SubmitSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Contains("verify your email", result.ErrorMessage);
    }

    [Fact]
    public async Task SubmitSolutionAsync_WhenSuccessful_InvalidatesPersonalizedProblemsCache()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = "Console.WriteLine(\"hi\");",
            Language = LanguageType.CSharp
        };

        var problem = new Problem
        {
            Id = problemId,
            IsDeleted = false,
            IsPublished = true,
            Name = "Test",
            Difficulty = ProblemDifficulty.Easy,
            ExplanationArticle = new Article { Title = "t", Content = "c" },
            Drivers =
            [
                new ProblemDriver { Language = LanguageType.CSharp, DriverCode = "", UITemplate = "", Answer = "", Image = "" }
            ]
        };

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        var submission = new ProblemSubmission
        {
            Id = 123,
            UserId = 1,
            ProblemId = problemId,
            Language = LanguageType.CSharp,
            CreatedAt = DateTimeOffset.UtcNow,
            Pass = true,
            TestCaseCount = 1,
            PassedTestCases = 1,
            FailedTestCases = 0,
            ExecutionTimeInMs = 1,
            ErrorMessage = null,
            TestCaseResults = []
        };

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(1, request.UserCode, request.Language, problem, It.IsAny<CancellationToken>()))
            .ReturnsAsync((submission, ""));

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemSubmissionService.SubmitSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        this.mockCache.Verify(x => x.Remove(CacheKeys.GetPersonalizedProblemsKey(1)), Times.Once);
    }

    #endregion

    #region RunSolutionAsync Tests

    [Fact]
    public async Task RunSolutionAsync_WithNullUserCode_ReturnsValidationError()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = null!,
            Language = LanguageType.CSharp
        };

        // Act
        var result = await this.problemSubmissionService.RunSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("User code is required", result.ErrorMessage);
    }

    [Fact]
    public async Task RunSolutionAsync_WithEmptyUserCode_ReturnsValidationError()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = "",
            Language = LanguageType.CSharp
        };

        // Act
        var result = await this.problemSubmissionService.RunSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("User code is required", result.ErrorMessage);
    }

    #endregion

    #region XP Award Tests

    [Fact]
    public async Task SubmitSolutionAsync_WithPassingSubmission_AwardsXpAndReturnsXpResult()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = "Console.WriteLine(\"hi\");",
            Language = LanguageType.CSharp
        };

        var problem = this.CreateTestProblem(problemId, ProblemDifficulty.Easy);

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        var submission = this.CreateTestSubmission(problemId, pass: true);

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(1, request.UserCode, request.Language, problem, It.IsAny<CancellationToken>()))
            .ReturnsAsync((submission, ""));

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        this.mockProgressionService
            .Setup(x => x.CalculateEarnedXp(ProblemDifficulty.Easy, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
            .Returns(75);

        this.mockProgressionService
            .Setup(x => x.GetLevelProgress(75))
            .Returns(new LevelProgress(1, 75, 100));

        // Act
        var result = await this.problemSubmissionService.SubmitSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.ValueOrThrow;
        Assert.Equal(75, dto.Xp.XpEarnedThisSubmission);
        Assert.Equal(75, dto.Xp.XpDeltaApplied);
        Assert.Equal(75, dto.Xp.TotalXp);
        Assert.True(dto.Xp.IsXpEligibleThisSubmission);
        this.mockExperienceEventRepository.Verify(x => x.Add(It.Is<ExperienceEvent>(e => e.XpDelta == 75)), Times.Once);
    }

    [Fact]
    public async Task SubmitSolutionAsync_WithFailingSubmission_AwardsNoXp()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = "bad code",
            Language = LanguageType.CSharp
        };

        var problem = this.CreateTestProblem(problemId, ProblemDifficulty.Easy);

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        var submission = this.CreateTestSubmission(problemId, pass: false);

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(1, request.UserCode, request.Language, problem, It.IsAny<CancellationToken>()))
            .ReturnsAsync((submission, ""));

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemSubmissionService.SubmitSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.ValueOrThrow;
        Assert.Equal(0, dto.Xp.XpEarnedThisSubmission);
        Assert.Equal(0, dto.Xp.XpDeltaApplied);
        Assert.False(dto.Xp.IsXpEligibleThisSubmission);
    }

    [Fact]
    public async Task SubmitSolutionAsync_WithExistingBestXp_OnlyAppliesDelta()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = "Console.WriteLine(\"hi\");",
            Language = LanguageType.CSharp
        };

        var problem = this.CreateTestProblem(problemId, ProblemDifficulty.Medium);

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        var existingProgress = new UserProblemProgress
        {
            UserId = 1,
            ProblemId = problemId,
            BestXp = 40,
            FirstXpAwardedAt = DateTimeOffset.UtcNow.AddMonths(-2),
            LastAwardedCycleIndex = 0,
            LastSolvedAt = DateTimeOffset.UtcNow.AddMonths(-2)
        };

        this.mockUserProblemProgressRepository
            .Setup(x => x.GetByUserAndProblemAsync(1, problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProgress);

        this.mockProgressionService
            .Setup(x => x.GetCycleIndex(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Returns(2);

        var submission = this.CreateTestSubmission(problemId, pass: true);

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(1, request.UserCode, request.Language, problem, It.IsAny<CancellationToken>()))
            .ReturnsAsync((submission, ""));

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        this.mockProgressionService
            .Setup(x => x.CalculateEarnedXp(ProblemDifficulty.Medium, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
            .Returns(60);

        this.mockProgressionService
            .Setup(x => x.GetLevelProgress(20))
            .Returns(new LevelProgress(1, 20, 100));

        // Act
        var result = await this.problemSubmissionService.SubmitSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.ValueOrThrow;
        Assert.Equal(60, dto.Xp.XpEarnedThisSubmission);
        Assert.Equal(20, dto.Xp.XpDeltaApplied); // 60 - 40 = 20 delta
        Assert.Equal(60, dto.Xp.ProblemBestXp);
        Assert.True(dto.Xp.IsXpEligibleThisSubmission);
    }

    [Fact]
    public async Task SubmitSolutionAsync_DuringCooldown_AwardsNoXp()
    {
        // Arrange
        var problemId = 1;
        var request = new ProblemSubmissionRequest
        {
            UserCode = "Console.WriteLine(\"hi\");",
            Language = LanguageType.CSharp
        };

        var problem = this.CreateTestProblem(problemId, ProblemDifficulty.Easy);

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        var existingProgress = new UserProblemProgress
        {
            UserId = 1,
            ProblemId = problemId,
            BestXp = 50,
            FirstXpAwardedAt = DateTimeOffset.UtcNow.AddDays(-10),
            LastAwardedCycleIndex = 0,
            LastSolvedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };

        this.mockUserProblemProgressRepository
            .Setup(x => x.GetByUserAndProblemAsync(1, problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProgress);

        // Same cycle — not eligible
        this.mockProgressionService
            .Setup(x => x.GetCycleIndex(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Returns(0);

        var submission = this.CreateTestSubmission(problemId, pass: true);

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(1, request.UserCode, request.Language, problem, It.IsAny<CancellationToken>()))
            .ReturnsAsync((submission, ""));

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemSubmissionService.SubmitSolutionAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.ValueOrThrow;
        Assert.Equal(0, dto.Xp.XpDeltaApplied);
        Assert.False(dto.Xp.IsXpEligibleThisSubmission);
        Assert.NotNull(dto.Xp.NextEligibleAt);
    }

    #endregion

    #region OpenHintAsync Tests

    [Fact]
    public async Task OpenHintAsync_AlwaysTracksHintRegardlessOfCycle()
    {
        // Arrange
        var problemId = 1;
        var hintIndex = 0;

        var problem = this.CreateTestProblem(problemId, ProblemDifficulty.Easy);
        problem.Hints = ["hint1", "hint2"];

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        // Progress in cooldown (same cycle as last award)
        var existingProgress = new UserProblemProgress
        {
            UserId = 1,
            ProblemId = problemId,
            BestXp = 50,
            FirstXpAwardedAt = DateTimeOffset.UtcNow.AddDays(-5),
            LastAwardedCycleIndex = 0,
            FirstSubmitAtCurrentCycle = DateTimeOffset.UtcNow.AddDays(-5),
            OpenedHintIndicesCurrentCycle = []
        };

        this.mockUserProblemProgressRepository
            .Setup(x => x.GetByUserAndProblemAsync(1, problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProgress);

        this.mockProgressionService
            .Setup(x => x.GetCycleIndex(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
            .Returns(0); // Same cycle — cooldown

        this.mockUserProblemProgressRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemSubmissionService.OpenHintAsync(problemId, hintIndex, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(hintIndex, existingProgress.OpenedHintIndicesCurrentCycle);
    }

    [Fact]
    public async Task OpenHintAsync_WithInvalidHintIndex_ReturnsValidationError()
    {
        // Arrange
        var problemId = 1;
        var hintIndex = -1;

        // Act
        var result = await this.problemSubmissionService.OpenHintAsync(problemId, hintIndex, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task OpenHintAsync_WithOutOfRangeHintIndex_ReturnsValidationError()
    {
        // Arrange
        var problemId = 1;
        var hintIndex = 5;

        var problem = this.CreateTestProblem(problemId, ProblemDifficulty.Easy);
        problem.Hints = ["only one hint"];

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        // Act
        var result = await this.problemSubmissionService.OpenHintAsync(problemId, hintIndex, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
    }

    #endregion

    #region Helpers

    private Problem CreateTestProblem(int id, ProblemDifficulty difficulty)
    {
        return new Problem
        {
            Id = id,
            IsDeleted = false,
            IsPublished = true,
            Name = "Test Problem",
            Difficulty = difficulty,
            ExplanationArticle = new Article { Title = "t", Content = "c" },
            Drivers =
            [
                new ProblemDriver { Language = LanguageType.CSharp, DriverCode = "", UITemplate = "", Answer = "", Image = "" }
            ]
        };
    }

    private ProblemSubmission CreateTestSubmission(int problemId, bool pass)
    {
        return new ProblemSubmission
        {
            Id = 123,
            UserId = 1,
            ProblemId = problemId,
            Language = LanguageType.CSharp,
            CreatedAt = DateTimeOffset.UtcNow,
            Pass = pass,
            TestCaseCount = 1,
            PassedTestCases = pass ? 1 : 0,
            FailedTestCases = pass ? 0 : 1,
            ExecutionTimeInMs = 100,
            ErrorMessage = null,
            TestCaseResults = []
        };
    }

    #endregion
}
