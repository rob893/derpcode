using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
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

    private readonly ProblemSubmissionService problemSubmissionService;

    public ProblemSubmissionServiceTests()
    {
        this.mockLogger = new Mock<ILogger<ProblemSubmissionService>>();
        this.mockCodeExecutionService = new Mock<ICodeExecutionService>();
        this.mockProblemRepository = new Mock<IProblemRepository>();
        this.mockProblemSubmissionRepository = new Mock<IProblemSubmissionRepository>();
        this.mockCurrentUserService = new Mock<ICurrentUserService>();

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.EmailVerified).Returns(true);

        this.problemSubmissionService = new ProblemSubmissionService(
            this.mockLogger.Object,
            this.mockCodeExecutionService.Object,
            this.mockProblemRepository.Object,
            this.mockProblemSubmissionRepository.Object,
            this.mockCurrentUserService.Object);
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
