using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Domain;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Tests.Services.Domain;

/// <summary>
/// Unit tests for the <see cref="UserSubmissionService"/> class
/// </summary>
public sealed class UserSubmissionServiceTests
{
    private readonly Mock<IProblemSubmissionRepository> mockProblemSubmissionRepository;

    private readonly Mock<ICurrentUserService> mockCurrentUserService;

    private readonly Mock<ILogger<UserSubmissionService>> mockLogger;

    private readonly UserSubmissionService userSubmissionService;

    public UserSubmissionServiceTests()
    {
        this.mockProblemSubmissionRepository = new Mock<IProblemSubmissionRepository>();
        this.mockCurrentUserService = new Mock<ICurrentUserService>();
        this.mockLogger = new Mock<ILogger<UserSubmissionService>>();

        this.userSubmissionService = new UserSubmissionService(
            this.mockProblemSubmissionRepository.Object,
            this.mockCurrentUserService.Object,
            this.mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserSubmissionService(null!, this.mockCurrentUserService.Object, this.mockLogger.Object));

        Assert.Equal("problemSubmissionRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCurrentUserService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserSubmissionService(this.mockProblemSubmissionRepository.Object, null!, this.mockLogger.Object));

        Assert.Equal("currentUserService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserSubmissionService(this.mockProblemSubmissionRepository.Object, this.mockCurrentUserService.Object, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new UserSubmissionService(this.mockProblemSubmissionRepository.Object, this.mockCurrentUserService.Object, this.mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region GetUserSubmissionsAsync Tests

    [Fact]
    public async Task GetUserSubmissionsAsync_WithNullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userSubmissionService.GetUserSubmissionsAsync(1, null!, CancellationToken.None));

        Assert.Equal("searchParams", exception.ParamName);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_WithValidParams_ReturnsSubmissions()
    {
        // Arrange
        var userId = 1;
        var searchParams = new UserSubmissionQueryParameters { First = 10 };
        var submissions = new List<ProblemSubmission> { CreateTestProblemSubmission(userId) };
        var pagedList = new CursorPaginatedList<ProblemSubmission, long>(submissions, false, false, "1", "1", 1);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);
        this.mockProblemSubmissionRepository
            .Setup(x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal(submissions[0].Id, result.Value.First().Id);
        Assert.Equal(submissions[0].UserId, result.Value.First().UserId);
        Assert.Equal(submissions[0].ProblemId, result.Value.First().ProblemId);
        Assert.Equal(submissions[0].Language, result.Value.First().Language);
        Assert.Equal(submissions[0].Code, result.Value.First().Code);
        Assert.Equal(submissions[0].Pass, result.Value.First().Pass);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_WithProblemIdFilter_PassesCorrectParameters()
    {
        // Arrange
        var userId = 1;
        var problemId = 5;
        var searchParams = new UserSubmissionQueryParameters { ProblemId = problemId, First = 10 };
        var submissions = new List<ProblemSubmission>();
        var pagedList = new CursorPaginatedList<ProblemSubmission, long>(submissions, false, false, null, null, 0);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);
        this.mockProblemSubmissionRepository
            .Setup(x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        await this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, CancellationToken.None);

        // Assert
        this.mockProblemSubmissionRepository.Verify(
            x => x.SearchAsync(
                It.Is<ProblemSubmissionQueryParameters>(p =>
                    p.UserId == userId &&
                    p.ProblemId == problemId &&
                    p.First == searchParams.First),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_WithPaginationParameters_PassesCorrectParameters()
    {
        // Arrange
        var userId = 1;
        var searchParams = new UserSubmissionQueryParameters
        {
            First = 20,
            After = "cursor1",
            Before = "cursor2",
            Last = 10,
            IncludeTotal = true,
            IncludeNodes = true,
            IncludeEdges = false
        };
        var submissions = new List<ProblemSubmission>();
        var pagedList = new CursorPaginatedList<ProblemSubmission, long>(submissions, false, false, null, null, 0);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);
        this.mockProblemSubmissionRepository
            .Setup(x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        await this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, CancellationToken.None);

        // Assert
        this.mockProblemSubmissionRepository.Verify(
            x => x.SearchAsync(
                It.Is<ProblemSubmissionQueryParameters>(p =>
                    p.UserId == userId &&
                    p.First == searchParams.First &&
                    p.After == searchParams.After &&
                    p.Before == searchParams.Before &&
                    p.Last == searchParams.Last &&
                    p.IncludeTotal == searchParams.IncludeTotal &&
                    p.IncludeNodes == searchParams.IncludeNodes &&
                    p.IncludeEdges == searchParams.IncludeEdges),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_WithCancellationToken_PassesToRepository()
    {
        // Arrange
        var userId = 1;
        var searchParams = new UserSubmissionQueryParameters { First = 10 };
        var cancellationToken = new CancellationToken(true);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);
        this.mockProblemSubmissionRepository
            .Setup(x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, cancellationToken));

        this.mockProblemSubmissionRepository.Verify(
            x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_WithEmptyResults_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;
        var searchParams = new UserSubmissionQueryParameters { First = 10 };
        var submissions = new List<ProblemSubmission>();
        var pagedList = new CursorPaginatedList<ProblemSubmission, long>(submissions, false, false, null, null, 0);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);
        this.mockProblemSubmissionRepository
            .Setup(x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.False(result.Value.HasNextPage);
        Assert.False(result.Value.HasPreviousPage);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_WithMultipleResults_ReturnsAllMapped()
    {
        // Arrange
        var userId = 1;
        var searchParams = new UserSubmissionQueryParameters { First = 10 };
        var submissions = new List<ProblemSubmission>
        {
            CreateTestProblemSubmission(userId, 1, 1),
            CreateTestProblemSubmission(userId, 2, 2),
            CreateTestProblemSubmission(userId, 3, 1)
        };
        var pagedList = new CursorPaginatedList<ProblemSubmission, long>(submissions, true, false, "1", "3", 10);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);
        this.mockProblemSubmissionRepository
            .Setup(x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(3, result.Value.ToList().Count);
        Assert.Equal(10, result.Value.TotalCount);
        Assert.True(result.Value.HasNextPage);
        Assert.False(result.Value.HasPreviousPage);
        Assert.Equal("1", result.Value.StartCursor);
        Assert.Equal("3", result.Value.EndCursor);

        // Verify all submissions are properly mapped
        var resultList = result.Value.ToList();
        for (int i = 0; i < submissions.Count; i++)
        {
            Assert.Equal(submissions[i].Id, resultList[i].Id);
            Assert.Equal(submissions[i].UserId, resultList[i].UserId);
            Assert.Equal(submissions[i].ProblemId, resultList[i].ProblemId);
            Assert.Equal(submissions[i].Language, resultList[i].Language);
            Assert.Equal(submissions[i].Code, resultList[i].Code);
            Assert.Equal(submissions[i].CreatedAt, resultList[i].CreatedAt);
            Assert.Equal(submissions[i].Pass, resultList[i].Pass);
            Assert.Equal(submissions[i].TestCaseCount, resultList[i].TestCaseCount);
            Assert.Equal(submissions[i].PassedTestCases, resultList[i].PassedTestCases);
            Assert.Equal(submissions[i].FailedTestCases, resultList[i].FailedTestCases);
            Assert.Equal(submissions[i].ErrorMessage ?? string.Empty, resultList[i].ErrorMessage);
            Assert.Equal(submissions[i].ExecutionTimeInMs, resultList[i].ExecutionTimeInMs);
        }
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_CallsRepositoryWithTrackingDisabled()
    {
        // Arrange
        var userId = 1;
        var searchParams = new UserSubmissionQueryParameters { First = 10 };
        var submissions = new List<ProblemSubmission>();
        var pagedList = new CursorPaginatedList<ProblemSubmission, long>(submissions, false, false, null, null, 0);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);
        this.mockProblemSubmissionRepository
            .Setup(x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        await this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, CancellationToken.None);

        // Assert
        this.mockProblemSubmissionRepository.Verify(
            x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_WithNonAdminUserAccessingOwnSubmissions_ReturnsSubmissions()
    {
        // Arrange
        var userId = 123;
        var searchParams = new UserSubmissionQueryParameters { First = 10 };
        var submissions = new List<ProblemSubmission> { CreateTestProblemSubmission(userId) };
        var pagedList = new CursorPaginatedList<ProblemSubmission, long>(submissions, false, false, "1", "1", 1);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);
        this.mockProblemSubmissionRepository
            .Setup(x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal(submissions[0].Id, result.Value.First().Id);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_WithNonAdminUserAccessingOtherSubmissions_ReturnsForbidden()
    {
        // Arrange
        var currentUserId = 123;
        var targetUserId = 456;
        var searchParams = new UserSubmissionQueryParameters { First = 10 };

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(currentUserId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = await this.userSubmissionService.GetUserSubmissionsAsync(targetUserId, searchParams, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You do not have permission to view these submissions.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserSubmissionsAsync_WithAdminUserAccessingAnySubmissions_ReturnsSubmissions()
    {
        // Arrange
        var currentUserId = 123;
        var targetUserId = 456;
        var searchParams = new UserSubmissionQueryParameters { First = 10 };
        var submissions = new List<ProblemSubmission> { CreateTestProblemSubmission(targetUserId) };
        var pagedList = new CursorPaginatedList<ProblemSubmission, long>(submissions, false, false, "1", "1", 1);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(currentUserId);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(true);
        this.mockProblemSubmissionRepository
            .Setup(x => x.SearchAsync(It.IsAny<ProblemSubmissionQueryParameters>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.userSubmissionService.GetUserSubmissionsAsync(targetUserId, searchParams, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal(submissions[0].Id, result.Value.First().Id);
    }

    #endregion

    #region Helper Methods

    private static ProblemSubmission CreateTestProblemSubmission(int userId, long id = 1, int problemId = 1)
    {
        return new ProblemSubmission
        {
            Id = id,
            UserId = userId,
            ProblemId = problemId,
            Language = LanguageType.CSharp,
            Code = "public class Solution { public int Solve() { return 42; } }",
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
            Pass = true,
            TestCaseCount = 5,
            PassedTestCases = 5,
            FailedTestCases = 0,
            ErrorMessage = null,
            ExecutionTimeInMs = 150,
            MemoryKb = 1024,
            TestCaseResults = []
        };
    }

    #endregion
}
