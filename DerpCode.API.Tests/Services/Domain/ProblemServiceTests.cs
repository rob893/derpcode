using System.Linq.Expressions;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Tests.Services.Domain;

/// <summary>
/// Unit tests for the <see cref="ProblemService"/> class
/// </summary>
public sealed class ProblemServiceTests
{
    private readonly Mock<ILogger<ProblemService>> mockLogger;

    private readonly Mock<ICodeExecutionService> mockCodeExecutionService;

    private readonly Mock<IProblemRepository> mockProblemRepository;

    private readonly Mock<IMemoryCache> mockCache;

    private readonly Mock<ICurrentUserService> mockCurrentUserService;

    private readonly Mock<ITagRepository> mockTagRepository;

    private readonly ProblemService problemService;

    public ProblemServiceTests()
    {
        this.mockLogger = new Mock<ILogger<ProblemService>>();
        this.mockCodeExecutionService = new Mock<ICodeExecutionService>();
        this.mockProblemRepository = new Mock<IProblemRepository>();
        this.mockCurrentUserService = new Mock<ICurrentUserService>();
        this.mockCache = new Mock<IMemoryCache>();
        this.mockTagRepository = new Mock<ITagRepository>();

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(true);
        this.mockCurrentUserService.Setup(x => x.IsPremiumUser).Returns(false);

        this.problemService = new ProblemService(
            this.mockLogger.Object,
            this.mockCodeExecutionService.Object,
            this.mockProblemRepository.Object,
            this.mockTagRepository.Object,
            this.mockCurrentUserService.Object,
            this.mockCache.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProblemService(null!, this.mockCodeExecutionService.Object, this.mockProblemRepository.Object, this.mockTagRepository.Object, this.mockCurrentUserService.Object, this.mockCache.Object));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCodeExecutionService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProblemService(this.mockLogger.Object, null!, this.mockProblemRepository.Object, this.mockTagRepository.Object, this.mockCurrentUserService.Object, this.mockCache.Object));

        Assert.Equal("codeExecutionService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullProblemRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProblemService(this.mockLogger.Object, this.mockCodeExecutionService.Object, null!, this.mockTagRepository.Object, this.mockCurrentUserService.Object, this.mockCache.Object));

        Assert.Equal("problemRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCurrentUserService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProblemService(this.mockLogger.Object, this.mockCodeExecutionService.Object, this.mockProblemRepository.Object, this.mockTagRepository.Object, null!, this.mockCache.Object));

        Assert.Equal("currentUserService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullTagRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProblemService(this.mockLogger.Object, this.mockCodeExecutionService.Object, this.mockProblemRepository.Object, null!, this.mockCurrentUserService.Object, this.mockCache.Object));

        Assert.Equal("tagRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCache_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new ProblemService(this.mockLogger.Object, this.mockCodeExecutionService.Object, this.mockProblemRepository.Object, this.mockTagRepository.Object, this.mockCurrentUserService.Object, null!));

        Assert.Equal("cache", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new ProblemService(
            this.mockLogger.Object,
            this.mockCodeExecutionService.Object,
            this.mockProblemRepository.Object,
            this.mockTagRepository.Object,
            this.mockCurrentUserService.Object,
            this.mockCache.Object);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region GetProblemsAsync Tests

    [Fact]
    public async Task GetProblemsAsync_WithNullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.problemService.GetProblemsAsync(null!, CancellationToken.None));

        Assert.Equal("searchParams", exception.ParamName);
    }

    [Fact]
    public async Task GetProblemsAsync_WithValidParams_ReturnsProblems()
    {
        // Arrange
        var searchParams = new ProblemQueryParameters { First = 10 };
        var testProblem = CreateTestProblem();
        testProblem.IsPublished = true; // Make sure it's published so it passes the filter
        var problems = new List<Problem> { testProblem };

        // Mock cache miss and repository call for loading all problems
        this.mockCache
            .Setup(x => x.TryGetValue(CacheKeys.Problems, out It.Ref<object?>.IsAny))
            .Returns(false);

        this.mockProblemRepository
            .Setup(x => x.SearchAsync(
                It.IsAny<Expression<Func<Problem, bool>>>(),
                It.IsAny<Expression<Func<Problem, object>>[]>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(problems);

        var mockCacheEntry = new Mock<ICacheEntry>();
        this.mockCache
            .Setup(x => x.CreateEntry(CacheKeys.Problems))
            .Returns(mockCacheEntry.Object);

        // Act
        var result = await this.problemService.GetProblemsAsync(searchParams, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(problems[0].Id, result.First().Id);
        Assert.Equal(problems[0].Name, result.First().Name);
    }

    [Fact]
    public async Task GetProblemsAsync_WithCancellationToken_PassesToRepository()
    {
        // Arrange
        var searchParams = new ProblemQueryParameters { First = 10 };
        var cancellationToken = new CancellationToken(true);

        // Mock cache miss
        this.mockCache
            .Setup(x => x.TryGetValue(CacheKeys.Problems, out It.Ref<object?>.IsAny))
            .Returns(false);

        this.mockProblemRepository
            .Setup(x => x.SearchAsync(
                It.IsAny<Expression<Func<Problem, bool>>>(),
                It.IsAny<Expression<Func<Problem, object>>[]>(),
                false,
                cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            this.problemService.GetProblemsAsync(searchParams, cancellationToken));

        this.mockProblemRepository.Verify(x => x.SearchAsync(
            It.IsAny<Expression<Func<Problem, bool>>>(),
            It.IsAny<Expression<Func<Problem, object>>[]>(),
            false,
            cancellationToken), Times.Once);
    }

    #endregion

    #region GetProblemByIdAsync Tests

    [Fact]
    public async Task GetProblemByIdAsync_WhenProblemExists_ReturnsProblemDto()
    {
        // Arrange
        var problemId = 1;
        var problem = CreateTestProblem();
        var problems = new List<Problem> { problem };

        // Mock cache miss and repository call for loading all problems
        this.mockCache
            .Setup(x => x.TryGetValue(CacheKeys.Problems, out It.Ref<object?>.IsAny))
            .Returns(false);

        this.mockProblemRepository
            .Setup(x => x.SearchAsync(
                It.IsAny<Expression<Func<Problem, bool>>>(),
                It.IsAny<Expression<Func<Problem, object>>[]>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(problems);

        var mockCacheEntry = new Mock<ICacheEntry>();
        this.mockCache
            .Setup(x => x.CreateEntry(CacheKeys.Problems))
            .Returns(mockCacheEntry.Object);

        // Act
        var result = await this.problemService.GetProblemByIdAsync(problemId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(problem.Id, result.Id);
        Assert.Equal(problem.Name, result.Name);
    }

    [Fact]
    public async Task GetProblemByIdAsync_WhenProblemNotFound_ReturnsNull()
    {
        // Arrange
        var problemId = 999;
        var problems = new List<Problem> { CreateTestProblem() }; // Problem with Id = 1, not 999

        // Mock cache miss and repository call for loading all problems
        this.mockCache
            .Setup(x => x.TryGetValue(CacheKeys.Problems, out It.Ref<object?>.IsAny))
            .Returns(false);

        this.mockProblemRepository
            .Setup(x => x.SearchAsync(
                It.IsAny<Expression<Func<Problem, bool>>>(),
                It.IsAny<Expression<Func<Problem, object>>[]>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(problems);

        var mockCacheEntry = new Mock<ICacheEntry>();
        this.mockCache
            .Setup(x => x.CreateEntry(CacheKeys.Problems))
            .Returns(mockCacheEntry.Object);

        // Act
        var result = await this.problemService.GetProblemByIdAsync(problemId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProblemByIdAsync_WithCancellationToken_PassesToRepository()
    {
        // Arrange
        var problemId = 1;
        var cancellationToken = new CancellationToken(true);

        // Mock cache miss
        this.mockCache
            .Setup(x => x.TryGetValue(CacheKeys.Problems, out It.Ref<object?>.IsAny))
            .Returns(false);

        this.mockProblemRepository
            .Setup(x => x.SearchAsync(
                It.IsAny<Expression<Func<Problem, bool>>>(),
                It.IsAny<Expression<Func<Problem, object>>[]>(),
                false,
                cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            this.problemService.GetProblemByIdAsync(problemId, cancellationToken));

        this.mockProblemRepository.Verify(x => x.SearchAsync(
            It.IsAny<Expression<Func<Problem, bool>>>(),
            It.IsAny<Expression<Func<Problem, object>>[]>(),
            false,
            cancellationToken), Times.Once);
    }

    #endregion

    #region CreateProblemAsync Tests

    [Fact]
    public async Task CreateProblemAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.problemService.CreateProblemAsync(null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task CreateProblemAsync_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = CreateTestCreateProblemRequest();

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemService.CreateProblemAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(request.Name, result.Value.Name);
        this.mockProblemRepository.Verify(x => x.Add(It.IsAny<Problem>()), Times.Once);
        this.mockProblemRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProblemAsync_WithExistingTags_UsesExistingTags()
    {
        // Arrange
        var request = CreateTestCreateProblemRequest();
        var existingTag = new Tag { Id = 1, Name = "existing" };
        var existingTags = new List<Tag> { existingTag };

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTags);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        Problem? capturedProblem = null;
        this.mockProblemRepository
            .Setup(x => x.Add(It.IsAny<Problem>()))
            .Callback<Problem>(p => capturedProblem = p);

        // Act
        var result = await this.problemService.CreateProblemAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedProblem);
        Assert.Contains(capturedProblem.Tags, t => t.Id == existingTag.Id);
    }

    [Fact]
    public async Task CreateProblemAsync_SetsCurrentUserAsExplanationArticleAuthor()
    {
        // Arrange
        var request = CreateTestCreateProblemRequest();
        var userId = 42;

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        Problem? capturedProblem = null;
        this.mockProblemRepository
            .Setup(x => x.Add(It.IsAny<Problem>()))
            .Callback<Problem>(p => capturedProblem = p);

        // Act
        var result = await this.problemService.CreateProblemAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedProblem);
        Assert.Equal(userId, capturedProblem.ExplanationArticle.UserId);
        Assert.Equal(userId, capturedProblem.ExplanationArticle.LastEditedById);
    }

    [Fact]
    public async Task CreateProblemAsync_WithCancellationToken_PassesToRepositories()
    {
        // Arrange
        var request = CreateTestCreateProblemRequest();
        var cancellationToken = new CancellationToken(true);

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            this.problemService.CreateProblemAsync(request, cancellationToken));

        this.mockTagRepository.Verify(x => x.SearchAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true, cancellationToken), Times.Once);
    }

    #endregion

    #region CloneProblemAsync Tests

    [Fact]
    public async Task CloneProblemAsync_WhenProblemNotFound_ReturnsFailureResult()
    {
        // Arrange
        var problemId = 999;

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Problem?)null);

        // Act
        var result = await this.problemService.CloneProblemAsync(problemId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Contains($"Problem with ID {problemId} not found", result.ErrorMessage);
    }

    [Fact]
    public async Task CloneProblemAsync_WhenProblemExists_ReturnsClonedProblem()
    {
        // Arrange
        var problemId = 1;
        var existingProblem = CreateTestProblem();

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProblem);

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemService.CloneProblemAsync(problemId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal($"{existingProblem.Name} (Clone)", result.Value.Name);
        Assert.Equal(existingProblem.Description, result.Value.Description);
    }

    [Fact]
    public async Task CloneProblemAsync_WithCancellationToken_PassesToRepository()
    {
        // Arrange
        var problemId = 1;
        var cancellationToken = new CancellationToken(true);

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            this.problemService.CloneProblemAsync(problemId, cancellationToken));

        this.mockProblemRepository.Verify(x => x.GetByIdAsync(problemId, true, cancellationToken), Times.Once);
    }

    #endregion

    #region PatchProblemAsync Tests

    [Fact]
    public async Task PatchProblemAsync_WithNullPatchDocument_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.problemService.PatchProblemAsync(1, null!, CancellationToken.None));

        Assert.Equal("patchDocument", exception.ParamName);
    }

    [Fact]
    public async Task PatchProblemAsync_WithEmptyPatchDocument_ReturnsFailureResult()
    {
        // Arrange
        var patchDocument = new JsonPatchDocument<CreateProblemRequest>();

        // Act
        var result = await this.problemService.PatchProblemAsync(1, patchDocument, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("at least 1 operation is required", result.ErrorMessage);
    }

    [Fact]
    public async Task PatchProblemAsync_WhenProblemNotFound_ReturnsFailureResult()
    {
        // Arrange
        var problemId = 999;
        var patchDocument = new JsonPatchDocument<CreateProblemRequest>();
        patchDocument.Replace(x => x.Name, "New Name");

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Problem?)null);

        // Act
        var result = await this.problemService.PatchProblemAsync(problemId, patchDocument, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Contains($"Problem with ID {problemId} not found", result.ErrorMessage);
    }

    [Fact]
    public async Task PatchProblemAsync_WithValidPatch_ReturnsSuccessResult()
    {
        // Arrange
        var problemId = 1;
        var problem = CreateTestProblem();
        var patchDocument = new JsonPatchDocument<CreateProblemRequest>();
        patchDocument.Replace(x => x.Name, "Updated Name");

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemService.PatchProblemAsync(problemId, patchDocument, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        this.mockProblemRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PatchProblemAsync_WhenSaveChangesFails_ReturnsFailureResult()
    {
        // Arrange
        var problemId = 1;
        var problem = CreateTestProblem();
        var patchDocument = new JsonPatchDocument<CreateProblemRequest>();
        patchDocument.Replace(x => x.Name, "Updated Name");

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await this.problemService.PatchProblemAsync(problemId, patchDocument, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Unknown, result.ErrorType);
        Assert.Contains("Failed to update problem", result.ErrorMessage);
    }

    #endregion

    #region UpdateProblemAsync Tests

    [Fact]
    public async Task UpdateProblemAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.problemService.UpdateProblemAsync(1, null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task UpdateProblemAsync_WhenProblemNotFound_ReturnsFailureResult()
    {
        // Arrange
        var problemId = 999;
        var request = CreateTestCreateProblemRequest();

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Problem?)null);

        // Act
        var result = await this.problemService.UpdateProblemAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Contains($"Problem with ID {problemId} not found", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateProblemAsync_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var problemId = 1;
        var existingProblem = CreateTestProblem();
        var request = CreateTestCreateProblemRequest();

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProblem);

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemService.UpdateProblemAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(request.Name, result.Value.Name);
        Assert.Equal(request.Description, result.Value.Description);
    }

    [Fact]
    public async Task UpdateProblemAsync_UpdatesDriversCorrectly()
    {
        // Arrange
        var problemId = 1;
        var existingProblem = CreateTestProblem();
        existingProblem.Drivers = [
            new ProblemDriver { Id = 1, Language = LanguageType.CSharp, Answer = "old answer", DriverCode = "old code", Image = "old image", UITemplate = "old template" }
        ];

        var request = CreateTestCreateProblemRequest();
        var updatedDrivers = new List<CreateProblemDriverRequest>(request.Drivers)
        {
            [0] = new CreateProblemDriverRequest
            {
                Language = LanguageType.CSharp,
                Answer = "new answer",
                DriverCode = "new code",
                Image = "new image",
                UITemplate = "new template"
            }
        };
        request = request with { Drivers = updatedDrivers };

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProblem);

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemService.UpdateProblemAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedDriver = existingProblem.Drivers.First();
        Assert.Equal("new answer", updatedDriver.Answer);
        Assert.Equal("new code", updatedDriver.DriverCode);
        Assert.Equal("new image", updatedDriver.Image);
        Assert.Equal("new template", updatedDriver.UITemplate);
    }

    [Fact]
    public async Task UpdateProblemAsync_WhenSaveChangesFails_ReturnsFailureResult()
    {
        // Arrange
        var problemId = 1;
        var existingProblem = CreateTestProblem();
        var request = CreateTestCreateProblemRequest();

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProblem);

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await this.problemService.UpdateProblemAsync(problemId, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Unknown, result.ErrorType);
        Assert.Contains("Failed to update problem", result.ErrorMessage);
    }

    #endregion

    #region DeleteProblemAsync Tests

    [Fact]
    public async Task DeleteProblemAsync_WhenProblemNotFound_ReturnsFailureResult()
    {
        // Arrange
        var problemId = 999;

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, It.IsAny<Expression<Func<Problem, object>>[]>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Problem?)null);

        // Act
        var result = await this.problemService.DeleteProblemAsync(problemId, false, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Contains($"Problem with ID {problemId} not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteProblemAsync_WhenProblemExists_ReturnsSuccessResult()
    {
        // Arrange
        var problemId = 1;
        var problem = CreateTestProblem();

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, It.IsAny<Expression<Func<Problem, object>>[]>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.problemService.DeleteProblemAsync(problemId, false, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.True(problem.IsDeleted); // Verify soft delete - IsDeleted should be true
        Assert.True(problem.ExplanationArticle.IsDeleted); // Explanation article should also be soft deleted
        this.mockProblemRepository.Verify(x => x.Remove(problem), Times.Never); // Should NOT call Remove for soft delete
        this.mockProblemRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        this.mockCache.Verify(x => x.Remove(CacheKeys.Problems), Times.Once); // Cache should be cleared
    }

    [Fact]
    public async Task DeleteProblemAsync_WhenSaveChangesFails_ReturnsFailureResult()
    {
        // Arrange
        var problemId = 1;
        var problem = CreateTestProblem();

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, It.IsAny<Expression<Func<Problem, object>>[]>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(problem);

        this.mockProblemRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await this.problemService.DeleteProblemAsync(problemId, false, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Unknown, result.ErrorType);
        Assert.Contains("Failed to delete problem", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteProblemAsync_WithCancellationToken_PassesToRepository()
    {
        // Arrange
        var problemId = 1;
        var cancellationToken = new CancellationToken(true);

        this.mockProblemRepository
            .Setup(x => x.GetByIdAsync(problemId, It.IsAny<Expression<Func<Problem, object>>[]>(), It.IsAny<bool>(), cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            this.problemService.DeleteProblemAsync(problemId, false, cancellationToken));

        this.mockProblemRepository.Verify(x => x.GetByIdAsync(problemId, It.IsAny<Expression<Func<Problem, object>>[]>(), It.IsAny<bool>(), cancellationToken), Times.Once);
    }

    #endregion

    #region ValidateCreateProblemAsync Tests

    [Fact]
    public async Task ValidateCreateProblemAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.problemService.ValidateCreateProblemAsync(null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task ValidateCreateProblemAsync_WithValidRequest_ReturnsValidationResponse()
    {
        // Arrange
        var request = CreateTestCreateProblemRequest();
        var submissionResult = CreateTestProblemSubmission();
        submissionResult.Pass = true;

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<LanguageType>(), It.IsAny<Problem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((submissionResult, "Test output"));

        // Act
        var result = await this.problemService.ValidateCreateProblemAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Single(result.DriverValidations);
        Assert.True(result.DriverValidations[0].IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateCreateProblemAsync_WhenDriverValidationFails_ReturnsInvalidResponse()
    {
        // Arrange
        var request = CreateTestCreateProblemRequest();
        var submissionResult = CreateTestProblemSubmission();
        submissionResult.Pass = false;
        submissionResult.ErrorMessage = "Test failed";

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<LanguageType>(), It.IsAny<Problem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((submissionResult, "Test output"));

        // Act
        var result = await this.problemService.ValidateCreateProblemAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Single(result.DriverValidations);
        Assert.False(result.DriverValidations[0].IsValid);
        Assert.Equal("Test failed", result.DriverValidations[0].ErrorMessage);
        Assert.Equal("One or more driver templates failed validation", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateCreateProblemAsync_WhenCodeExecutionThrows_ReturnsInvalidResponse()
    {
        // Arrange
        var request = CreateTestCreateProblemRequest();
        var exceptionMessage = "Code execution failed";

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<LanguageType>(), It.IsAny<Problem>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await this.problemService.ValidateCreateProblemAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Single(result.DriverValidations);
        Assert.False(result.DriverValidations[0].IsValid);
        Assert.Equal(exceptionMessage, result.DriverValidations[0].ErrorMessage);
        Assert.Equal("One or more driver templates failed validation", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateCreateProblemAsync_WithMultipleDrivers_ValidatesAll()
    {
        // Arrange
        var request = CreateTestCreateProblemRequest();
        var updatedDrivers = new List<CreateProblemDriverRequest>(request.Drivers)
        {
            new CreateProblemDriverRequest
            {
                Language = LanguageType.JavaScript,
                Answer = "test answer js",
                DriverCode = "test driver js",
                Image = "test-js-image",
                UITemplate = "test template js"
            }
        };
        request = request with { Drivers = updatedDrivers };

        var submissionResult = CreateTestProblemSubmission();
        submissionResult.Pass = true;

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<LanguageType>(), It.IsAny<Problem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((submissionResult, "Test output"));

        // Act
        var result = await this.problemService.ValidateCreateProblemAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Equal(2, result.DriverValidations.Count);
        Assert.All(result.DriverValidations, dv => Assert.True(dv.IsValid));

        // Verify code execution was called for each driver
        this.mockCodeExecutionService.Verify(
            x => x.RunCodeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<LanguageType>(), It.IsAny<Problem>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ValidateCreateProblemAsync_WithCancellationToken_HandlesException()
    {
        // Arrange
        var request = CreateTestCreateProblemRequest();
        var cancellationToken = new CancellationToken(true);

        this.mockCodeExecutionService
            .Setup(x => x.RunCodeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<LanguageType>(), It.IsAny<Problem>(), cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await this.problemService.ValidateCreateProblemAsync(request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.DriverValidations);
        Assert.All(result.DriverValidations, dv => Assert.False(dv.IsValid));

        this.mockCodeExecutionService.Verify(
            x => x.RunCodeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<LanguageType>(), It.IsAny<Problem>(), cancellationToken),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Problem CreateTestProblem()
    {
        return new Problem
        {
            Id = 1,
            Name = "Test Problem",
            Description = "This is a test problem",
            Difficulty = ProblemDifficulty.Easy,
            ExpectedOutput = [1, 2, 3],
            Input = ["a", "b", "c"],
            Hints = ["Hint 1", "Hint 2"],
            Tags = [new Tag { Id = 1, Name = "test" }],
            Drivers = [
                new ProblemDriver
                {
                    Id = 1,
                    Language = LanguageType.CSharp,
                    Answer = "test answer",
                    DriverCode = "test driver",
                    Image = "test-image",
                    UITemplate = "test template"
                }
            ],
            ExplanationArticle = new Article
            {
                Id = 1,
                Title = "Test Explanation",
                Content = "This is a test explanation",
                Type = ArticleType.ProblemSolution,
                UserId = 1,
                LastEditedById = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Tags = []
            },
            ExplanationArticleId = 1,
            SolutionArticles = [],
            ProblemSubmissions = []
        };
    }

    private static CreateProblemRequest CreateTestCreateProblemRequest()
    {
        return new CreateProblemRequest
        {
            Name = "Test Problem",
            Description = "This is a test problem",
            Difficulty = ProblemDifficulty.Easy,
            ExpectedOutput = [1, 2, 3],
            Input = ["a", "b", "c"],
            Hints = ["Hint 1", "Hint 2"],
            Tags = [new CreateTagRequest { Name = "existing" }],
            Drivers = [
                new CreateProblemDriverRequest
                {
                    Language = LanguageType.CSharp,
                    Answer = "test answer",
                    DriverCode = "test driver",
                    Image = "test-image",
                    UITemplate = "test template"
                }
            ],
            ExplanationArticle = new CreateProblemExplanationArticleRequest
            {
                Title = "Test Explanation",
                Content = "This is a test explanation"
            }
        };
    }

    private static ProblemSubmission CreateTestProblemSubmission()
    {
        return new ProblemSubmission
        {
            Id = 1,
            UserId = 1,
            ProblemId = 1,
            Language = LanguageType.CSharp,
            Code = "test code",
            CreatedAt = DateTimeOffset.UtcNow,
            Pass = true,
            TestCaseCount = 3,
            PassedTestCases = 3,
            FailedTestCases = 0,
            ErrorMessage = "",
            ExecutionTimeInMs = 100,
            TestCaseResults = []
        };
    }

    #endregion
}
