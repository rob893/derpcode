using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Tests.Services.Domain;

/// <summary>
/// Unit tests for the <see cref="UserFavoriteService"/> class
/// </summary>
public sealed class UserFavoriteServiceTests
{
    private readonly Mock<ILogger<UserFavoriteService>> mockLogger;

    private readonly Mock<IUserRepository> mockUserRepository;

    private readonly Mock<ICurrentUserService> mockCurrentUserService;

    private readonly Mock<IMemoryCache> mockCache;

    private readonly UserFavoriteService userFavoriteService;

    public UserFavoriteServiceTests()
    {
        this.mockLogger = new Mock<ILogger<UserFavoriteService>>();
        this.mockUserRepository = new Mock<IUserRepository>();
        this.mockCurrentUserService = new Mock<ICurrentUserService>();
        this.mockCache = new Mock<IMemoryCache>();

        this.userFavoriteService = new UserFavoriteService(
            this.mockLogger.Object,
            this.mockUserRepository.Object,
            this.mockCurrentUserService.Object,
            this.mockCache.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserFavoriteService(null!, this.mockUserRepository.Object, this.mockCurrentUserService.Object, this.mockCache.Object));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullUserRepository_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserFavoriteService(this.mockLogger.Object, null!, this.mockCurrentUserService.Object, this.mockCache.Object));

        Assert.Equal("userRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCurrentUserService_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserFavoriteService(this.mockLogger.Object, this.mockUserRepository.Object, null!, this.mockCache.Object));

        Assert.Equal("currentUserService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCache_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserFavoriteService(this.mockLogger.Object, this.mockUserRepository.Object, this.mockCurrentUserService.Object, null!));

        Assert.Equal("cache", exception.ParamName);
    }

    #endregion

    #region GetFavoriteProblemsForUserAsync Tests

    [Fact]
    public async Task GetFavoriteProblemsForUserAsync_WithDifferentUserAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        this.SetupCurrentUser(userId: 2, isAdmin: false);

        // Act
        var result = await this.userFavoriteService.GetFavoriteProblemsForUserAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You can only favorite problems for yourself.", result.ErrorMessage);

        this.mockUserRepository.Verify(
            x => x.GetFavoriteProblemsForUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        this.VerifyLoggerWasCalled(LogLevel.Warning, "attempted to access user");
    }

    [Fact]
    public async Task GetFavoriteProblemsForUserAsync_WithSameUser_ReturnsDtos()
    {
        // Arrange
        this.SetupCurrentUser(userId: 123, isAdmin: false);

        var createdAt1 = DateTimeOffset.UtcNow.AddMinutes(-10);
        var createdAt2 = DateTimeOffset.UtcNow.AddMinutes(-5);

        var favorites = new List<UserFavoriteProblem>
        {
            new() { UserId = 123, ProblemId = 10, CreatedAt = createdAt1 },
            new() { UserId = 123, ProblemId = 11, CreatedAt = createdAt2 }
        };

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        this.mockUserRepository
            .Setup(x => x.GetFavoriteProblemsForUserAsync(123, token))
            .ReturnsAsync(favorites);

        // Act
        var result = await this.userFavoriteService.GetFavoriteProblemsForUserAsync(123, token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);

        Assert.Equal(123, result.Value[0].UserId);
        Assert.Equal(10, result.Value[0].ProblemId);
        Assert.Equal(createdAt1, result.Value[0].CreatedAt);

        Assert.Equal(123, result.Value[1].UserId);
        Assert.Equal(11, result.Value[1].ProblemId);
        Assert.Equal(createdAt2, result.Value[1].CreatedAt);

        this.mockUserRepository.Verify(x => x.GetFavoriteProblemsForUserAsync(123, token), Times.Once);
    }

    [Fact]
    public async Task GetFavoriteProblemsForUserAsync_WithDifferentUserButAdmin_ReturnsSuccess()
    {
        // Arrange
        this.SetupCurrentUser(userId: 999, isAdmin: true);

        var favorites = new List<UserFavoriteProblem>
        {
            new() { UserId = 123, ProblemId = 10, CreatedAt = DateTimeOffset.UtcNow }
        };

        this.mockUserRepository
            .Setup(x => x.GetFavoriteProblemsForUserAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(favorites);

        // Act
        var result = await this.userFavoriteService.GetFavoriteProblemsForUserAsync(123, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);

        this.mockUserRepository.Verify(
            x => x.GetFavoriteProblemsForUserAsync(123, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region FavoriteProblemForUserAsync Tests

    [Fact]
    public async Task FavoriteProblemForUserAsync_WithDifferentUserAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        this.SetupCurrentUser(userId: 2, isAdmin: false);

        // Act
        var result = await this.userFavoriteService.FavoriteProblemForUserAsync(1, 99, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You can only favorite problems for yourself.", result.ErrorMessage);

        this.mockUserRepository.Verify(
            x => x.FavoriteProblemForUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        this.VerifyLoggerWasCalled(LogLevel.Warning, "attempted to access user");
    }

    [Fact]
    public async Task FavoriteProblemForUserAsync_WithSameUser_ReturnsFavoriteDto()
    {
        // Arrange
        this.SetupCurrentUser(userId: 123, isAdmin: false);

        var createdAt = DateTimeOffset.UtcNow;
        var favorite = new UserFavoriteProblem
        {
            UserId = 123,
            ProblemId = 77,
            CreatedAt = createdAt
        };

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        this.mockUserRepository
            .Setup(x => x.FavoriteProblemForUserAsync(123, 77, token))
            .ReturnsAsync(favorite);

        // Act
        var result = await this.userFavoriteService.FavoriteProblemForUserAsync(123, 77, token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(123, result.Value.UserId);
        Assert.Equal(77, result.Value.ProblemId);
        Assert.Equal(createdAt, result.Value.CreatedAt);

        this.mockUserRepository.Verify(x => x.FavoriteProblemForUserAsync(123, 77, token), Times.Once);
    }

    [Fact]
    public async Task FavoriteProblemForUserAsync_WhenRepositoryThrowsKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        this.SetupCurrentUser(userId: 123, isAdmin: false);

        this.mockUserRepository
            .Setup(x => x.FavoriteProblemForUserAsync(123, 55, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await this.userFavoriteService.FavoriteProblemForUserAsync(123, 55, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Equal("User or problem not found.", result.ErrorMessage);

        this.VerifyLoggerWasCalled(LogLevel.Warning, "attempted to favorite a problem");
    }

    #endregion

    #region UnfavoriteProblemForUserAsync Tests

    [Fact]
    public async Task UnfavoriteProblemForUserAsync_WithDifferentUserAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        this.SetupCurrentUser(userId: 2, isAdmin: false);

        // Act
        var result = await this.userFavoriteService.UnfavoriteProblemForUserAsync(1, 99, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You can only unfavorite your own problems.", result.ErrorMessage);

        this.mockUserRepository.Verify(
            x => x.UnfavoriteProblemForUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        this.VerifyLoggerWasCalled(LogLevel.Warning, "attempted to access user");
    }

    [Fact]
    public async Task UnfavoriteProblemForUserAsync_WithSameUser_ReturnsSuccess()
    {
        // Arrange
        this.SetupCurrentUser(userId: 123, isAdmin: false);

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        this.mockUserRepository
            .Setup(x => x.UnfavoriteProblemForUserAsync(123, 77, token))
            .ReturnsAsync(true);

        // Act
        var result = await this.userFavoriteService.UnfavoriteProblemForUserAsync(123, 77, token);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.ValueOrThrow);

        this.mockUserRepository.Verify(x => x.UnfavoriteProblemForUserAsync(123, 77, token), Times.Once);
    }

    [Fact]
    public async Task UnfavoriteProblemForUserAsync_WithDifferentUserButAdmin_ReturnsSuccess()
    {
        // Arrange
        this.SetupCurrentUser(userId: 999, isAdmin: true);

        this.mockUserRepository
            .Setup(x => x.UnfavoriteProblemForUserAsync(123, 77, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await this.userFavoriteService.UnfavoriteProblemForUserAsync(123, 77, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.ValueOrThrow);

        this.mockUserRepository.Verify(
            x => x.UnfavoriteProblemForUserAsync(123, 77, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helpers

    private void SetupCurrentUser(int userId, bool isAdmin)
    {
        this.mockCurrentUserService.SetupGet(x => x.UserId).Returns(userId);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(isAdmin);
        this.mockCurrentUserService.SetupGet(x => x.IsAdmin).Returns(isAdmin);
    }

    private void VerifyLoggerWasCalled(LogLevel logLevel, string message)
    {
        this.mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
