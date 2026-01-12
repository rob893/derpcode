using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
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

    private readonly Mock<IMemoryCache> mockCache;

    private readonly ProblemSubmissionService problemSubmissionService;

    public ProblemSubmissionServiceTests()
    {
        this.mockLogger = new Mock<ILogger<ProblemSubmissionService>>();
        this.mockCodeExecutionService = new Mock<ICodeExecutionService>();
        this.mockProblemRepository = new Mock<IProblemRepository>();
        this.mockProblemSubmissionRepository = new Mock<IProblemSubmissionRepository>();
        this.mockCurrentUserService = new Mock<ICurrentUserService>();
        this.mockCache = new Mock<IMemoryCache>();

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.EmailVerified).Returns(true);

        this.problemSubmissionService = new ProblemSubmissionService(
            this.mockLogger.Object,
            this.mockCodeExecutionService.Object,
            this.mockProblemRepository.Object,
            this.mockProblemSubmissionRepository.Object,
            this.mockCurrentUserService.Object,
            this.mockCache.Object);
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
}
