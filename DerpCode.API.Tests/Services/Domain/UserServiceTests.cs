using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Domain;
using DerpCode.API.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Tests.Services.Domain;

/// <summary>
/// Unit tests for the <see cref="UserService"/> class
/// </summary>
public sealed class UserServiceTests
{
    private readonly Mock<ILogger<UserService>> mockLogger;

    private readonly Mock<IUserRepository> mockUserRepository;

    private readonly Mock<IEmailService> mockEmailService;

    private readonly Mock<ICurrentUserService> mockCurrentUserService;

    private readonly Mock<UserManager<User>> mockUserManager;

    private readonly UserService userService;

    public UserServiceTests()
    {
        this.mockLogger = new Mock<ILogger<UserService>>();
        this.mockUserRepository = new Mock<IUserRepository>();
        this.mockEmailService = new Mock<IEmailService>();
        this.mockCurrentUserService = new Mock<ICurrentUserService>();

        // Setup UserManager mock (requires more complex setup)
        var mockUserStore = new Mock<IUserStore<User>>();
        this.mockUserManager = new Mock<UserManager<User>>(mockUserStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Setup the repository to return the mock UserManager
        this.mockUserRepository.Setup(x => x.UserManager).Returns(this.mockUserManager.Object);

        this.userService = new UserService(
            this.mockLogger.Object,
            this.mockUserRepository.Object,
            this.mockEmailService.Object,
            this.mockCurrentUserService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserService(null!, this.mockUserRepository.Object, this.mockEmailService.Object, this.mockCurrentUserService.Object));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullUserRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserService(this.mockLogger.Object, null!, this.mockEmailService.Object, this.mockCurrentUserService.Object));

        Assert.Equal("userRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullEmailService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserService(this.mockLogger.Object, this.mockUserRepository.Object, null!, this.mockCurrentUserService.Object));

        Assert.Equal("emailService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCurrentUserService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserService(this.mockLogger.Object, this.mockUserRepository.Object, this.mockEmailService.Object, null!));

        Assert.Equal("currentUserService", exception.ParamName);
    }

    #endregion

    #region GetUsersAsync Tests

    [Fact]
    public async Task GetUsersAsync_WithNullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userService.GetUsersAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task GetUsersAsync_WithValidParams_ReturnsUsers()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 10 };
        var users = new List<User>
        {
            new() { Id = 1, UserName = "user1", Email = "user1@example.com" },
            new() { Id = 2, UserName = "user2", Email = "user2@example.com" }
        };
        var pagedList = new CursorPaginatedList<User, int>(users, false, false, "1", "2", 2);

        this.mockUserRepository.Setup(x => x.SearchAsync(searchParams, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.userService.GetUsersAsync(searchParams, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PageCount);
        Assert.Equal(2, result.TotalCount);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ReturnsUserDto()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "testuser", Email = "test@example.com" };
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.GetUserByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("testuser", result.Value.UserName);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingUser_ReturnsFailure()
    {
        // Arrange
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.GetUserByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithDifferentUserAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = await this.userService.GetUserByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You can only see your own user", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithDifferentUserButIsAdmin_ReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "testuser", Email = "test@example.com" };
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(true);

        // Act
        var result = await this.userService.GetUserByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("testuser", result.Value.UserName);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_WithExistingUser_ReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "testuser" };
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.DeleteUserAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        this.mockUserRepository.Verify(x => x.Remove(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistingUser_ReturnsFailure()
    {
        // Arrange
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.DeleteUserAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteUserAsync_WithSaveFailure_ReturnsFailure()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "testuser" };
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.DeleteUserAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Unknown, result.ErrorType);
        Assert.Equal("Failed to delete the user", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteUserAsync_WithDifferentUserAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = await this.userService.DeleteUserAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You can only delete your own user", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteUserAsync_WithDifferentUserButIsAdmin_ReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "testuser" };
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(true);

        // Act
        var result = await this.userService.DeleteUserAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        this.mockUserRepository.Verify(x => x.Remove(user), Times.Once);
    }

    #endregion

    #region DeleteUserLinkedAccountAsync Tests

    [Fact]
    public async Task DeleteUserLinkedAccountAsync_WithExistingLinkedAccount_ReturnsSuccess()
    {
        // Arrange
        var linkedAccount = new LinkedAccount { LinkedAccountType = LinkedAccountType.Google };
        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            LinkedAccounts = [linkedAccount]
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.DeleteUserLinkedAccountAsync(1, LinkedAccountType.Google, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Empty(user.LinkedAccounts);
    }

    [Fact]
    public async Task DeleteUserLinkedAccountAsync_WithNonExistingUser_ReturnsFailure()
    {
        // Arrange
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.DeleteUserLinkedAccountAsync(1, LinkedAccountType.Google, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteUserLinkedAccountAsync_WithNonExistingLinkedAccount_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            LinkedAccounts = []
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.DeleteUserLinkedAccountAsync(1, LinkedAccountType.Google, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Equal("No linked account of type Google found for user", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteUserLinkedAccountAsync_WithDifferentUserAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = await this.userService.DeleteUserLinkedAccountAsync(1, LinkedAccountType.Google, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You can only delete linked accounts for your own user", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteUserLinkedAccountAsync_WithDifferentUserButIsAdmin_ReturnsSuccess()
    {
        // Arrange
        var linkedAccount = new LinkedAccount { LinkedAccountType = LinkedAccountType.Google };
        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            LinkedAccounts = [linkedAccount]
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(true);

        // Act
        var result = await this.userService.DeleteUserLinkedAccountAsync(1, LinkedAccountType.Google, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Empty(user.LinkedAccounts);
    }

    #endregion

    #region GetRolesAsync Tests

    [Fact]
    public async Task GetRolesAsync_WithValidParams_ReturnsRoles()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 10 };
        var roles = new List<Role>
        {
            new() { Id = 1, Name = "Admin" },
            new() { Id = 2, Name = "User" }
        };
        var pagedList = new CursorPaginatedList<Role, int>(roles, false, false, "1", "2", 2);

        this.mockUserRepository.Setup(x => x.GetRolesAsync(searchParams, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.userService.GetRolesAsync(searchParams, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PageCount);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetRolesAsync_WithNullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userService.GetRolesAsync(null!, CancellationToken.None));
    }

    #endregion

    #region AddRolesToUserAsync Tests

    [Fact]
    public async Task AddRolesToUserAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new EditRoleRequest { RoleNames = ["Admin", "User"] };
        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            UserRoles = []
        };
        var roles = new List<Role>
        {
            new() { Id = 1, Name = "Admin" },
            new() { Id = 2, Name = "User" }
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserRepository.Setup(x => x.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.userService.AddRolesToUserAsync(1, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, user.UserRoles.Count);
    }

    [Fact]
    public async Task AddRolesToUserAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userService.AddRolesToUserAsync(1, null!, CancellationToken.None));
    }

    [Fact]
    public async Task AddRolesToUserAsync_WithEmptyRoleNames_ReturnsFailure()
    {
        // Arrange
        var request = new EditRoleRequest { RoleNames = [] };

        // Act
        var result = await this.userService.AddRolesToUserAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Equal("At least one role must be specified", result.ErrorMessage);
    }

    [Fact]
    public async Task AddRolesToUserAsync_WithNonExistingUser_ReturnsFailure()
    {
        // Arrange
        var request = new EditRoleRequest { RoleNames = ["Admin"] };
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.userService.AddRolesToUserAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    #endregion

    #region RemoveRolesFromUserAsync Tests

    [Fact]
    public async Task RemoveRolesFromUserAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new EditRoleRequest { RoleNames = ["Admin"] };
        var adminRole = new Role { Id = 1, Name = "Admin" };
        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            UserRoles = [new UserRole { RoleId = 1, Role = adminRole }]
        };
        var roles = new List<Role> { adminRole };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserRepository.Setup(x => x.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);
        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.userService.RemoveRolesFromUserAsync(1, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(user.UserRoles);
    }

    [Fact]
    public async Task RemoveRolesFromUserAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userService.RemoveRolesFromUserAsync(1, null!, CancellationToken.None));
    }

    [Fact]
    public async Task RemoveRolesFromUserAsync_WithEmptyRoleNames_ReturnsFailure()
    {
        // Arrange
        var request = new EditRoleRequest { RoleNames = [] };

        // Act
        var result = await this.userService.RemoveRolesFromUserAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Equal("At least one role must be specified", result.ErrorMessage);
    }

    #endregion

    #region UpdateUsernameAsync Tests

    [Fact]
    public async Task UpdateUsernameAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new UpdateUsernameRequest { NewUsername = "newusername" };
        var user = new User
        {
            Id = 1,
            UserName = "oldusername",
            LastLogin = DateTimeOffset.UtcNow.AddMinutes(-10),
            LastUsernameChange = DateTimeOffset.UtcNow.AddDays(-35)
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.SetUserNameAsync(user, "newusername"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<User, string>((u, newUsername) => u.UserName = newUsername);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.UpdateUsernameAsync(1, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("newusername", result.Value.UserName);
    }

    [Fact]
    public async Task UpdateUsernameAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userService.UpdateUsernameAsync(1, null!, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateUsernameAsync_WithNonExistingUser_ReturnsFailure()
    {
        // Arrange
        var request = new UpdateUsernameRequest { NewUsername = "newusername" };
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.UpdateUsernameAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUsernameAsync_WithOldLogin_ReturnsFailure()
    {
        // Arrange
        var request = new UpdateUsernameRequest { NewUsername = "newusername" };
        var user = new User
        {
            Id = 1,
            UserName = "oldusername",
            LastLogin = DateTimeOffset.UtcNow.AddHours(-1)
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.UpdateUsernameAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("must have authenticated within the last 30 minutes", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUsernameAsync_WithRecentUsernameChange_ReturnsFailure()
    {
        // Arrange
        var request = new UpdateUsernameRequest { NewUsername = "newusername" };
        var user = new User
        {
            Id = 1,
            UserName = "oldusername",
            LastLogin = DateTimeOffset.UtcNow.AddMinutes(-10),
            LastUsernameChange = DateTimeOffset.UtcNow.AddDays(-10)
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.UpdateUsernameAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("once every 30 days", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUsernameAsync_WithSameUsername_ReturnsFailure()
    {
        // Arrange
        var request = new UpdateUsernameRequest { NewUsername = "testuser" };
        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            LastLogin = DateTimeOffset.UtcNow.AddMinutes(-10),
            LastUsernameChange = DateTimeOffset.UtcNow.AddDays(-35)
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.UpdateUsernameAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("must be different from the current one", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUsernameAsync_WithDifferentUserAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        var request = new UpdateUsernameRequest { NewUsername = "newusername" };
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = await this.userService.UpdateUsernameAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You can only update your own username", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUsernameAsync_WithDifferentUserButIsAdmin_ReturnsSuccess()
    {
        // Arrange
        var request = new UpdateUsernameRequest { NewUsername = "newusername" };
        var user = new User
        {
            Id = 1,
            UserName = "oldusername",
            LastLogin = DateTimeOffset.UtcNow.AddMinutes(-10),
            LastUsernameChange = DateTimeOffset.UtcNow.AddDays(-35)
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.SetUserNameAsync(user, "newusername"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<User, string>((u, newUsername) => u.UserName = newUsername);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(true);

        // Act
        var result = await this.userService.UpdateUsernameAsync(1, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("newusername", result.Value.UserName);
    }

    #endregion

    #region UpdatePasswordAsync Tests

    [Fact]
    public async Task UpdatePasswordAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new UpdatePasswordRequest { OldPassword = "oldpass", NewPassword = "newpass" };
        var user = new User { Id = 1, UserName = "testuser" };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.ChangePasswordAsync(user, "oldpass", "newpass"))
            .ReturnsAsync(IdentityResult.Success);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.UpdatePasswordAsync(1, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userService.UpdatePasswordAsync(1, null!, CancellationToken.None));
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithNonExistingUser_ReturnsFailure()
    {
        // Arrange
        var request = new UpdatePasswordRequest { OldPassword = "oldpass", NewPassword = "newpass" };
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.UpdatePasswordAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var request = new UpdatePasswordRequest { OldPassword = "oldpass", NewPassword = "newpass" };
        var user = new User { Id = 1, UserName = "testuser" };
        var identityError = new IdentityError { Description = "Password is too weak" };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.ChangePasswordAsync(user, "oldpass", "newpass"))
            .ReturnsAsync(IdentityResult.Failed(identityError));
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.UpdatePasswordAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("Password is too weak", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithDifferentUserAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        var request = new UpdatePasswordRequest { OldPassword = "oldpass", NewPassword = "newpass" };
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = await this.userService.UpdatePasswordAsync(1, request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You can only update your own password", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WithDifferentUserButIsAdmin_ReturnsSuccess()
    {
        // Arrange
        var request = new UpdatePasswordRequest { OldPassword = "oldpass", NewPassword = "newpass" };
        var user = new User { Id = 1, UserName = "testuser" };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.ChangePasswordAsync(user, "oldpass", "newpass"))
            .ReturnsAsync(IdentityResult.Success);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(true);

        // Act
        var result = await this.userService.UpdatePasswordAsync(1, request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    #endregion

    #region SendEmailConfirmationAsync Tests

    [Fact]
    public async Task SendEmailConfirmationAsync_WithValidUser_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = false,
            LastEmailConfirmationSent = null
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");
        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.SendEmailConfirmationAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        this.mockEmailService.Verify(x => x.SendEmailConfirmationToUserAsync(user, "confirmation-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WithNonExistingUser_ReturnsFailure()
    {
        // Arrange
        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.SendEmailConfirmationAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);
        Assert.Equal("User not found", result.ErrorMessage);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WithNoEmail_ReturnsFailure()
    {
        // Arrange
        var user = new User { Id = 1, Email = null };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.SendEmailConfirmationAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Equal("User does not have an email address set", result.ErrorMessage);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WithAlreadyConfirmedEmail_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = true
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.SendEmailConfirmationAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Equal("User's email is already confirmed", result.ErrorMessage);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WithRecentEmailSent_ReturnsFailure()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = false,
            LastEmailConfirmationSent = DateTimeOffset.UtcNow.AddMinutes(-30)
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);

        // Act
        var result = await this.userService.SendEmailConfirmationAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("once per hour", result.ErrorMessage);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WithDifferentUserAndNotAdmin_ReturnsForbidden()
    {
        // Arrange
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(false);

        // Act
        var result = await this.userService.SendEmailConfirmationAsync(1, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);
        Assert.Equal("You can only send email confirmation links for your own account", result.ErrorMessage);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WithDifferentUserButIsAdmin_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = false,
            LastEmailConfirmationSent = null
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");
        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(2);
        this.mockCurrentUserService.Setup(x => x.IsAdmin).Returns(true);

        // Act
        var result = await this.userService.SendEmailConfirmationAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        this.mockEmailService.Verify(x => x.SendEmailConfirmationToUserAsync(user, "confirmation-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ForgotPasswordAsync Tests

    [Fact]
    public async Task ForgotPasswordAsync_WithValidUser_ReturnsSuccess()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = true,
            LastPasswordChange = DateTimeOffset.UtcNow.AddDays(-1)
        };

        this.mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        // Act
        var result = await this.userService.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        this.mockEmailService.Verify(x => x.SendResetPasswordToUserAsync(user, "reset-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userService.ForgotPasswordAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithNonExistingUser_ReturnsSuccessForSecurity()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };
        this.mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.userService.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess); // Always returns success to prevent user enumeration
        Assert.True(result.Value);
        this.mockEmailService.Verify(x => x.SendResetPasswordToUserAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithRecentPasswordChange_ReturnsSuccessForSecurity()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = true,
            LastPasswordChange = DateTimeOffset.UtcNow.AddMinutes(-30)
        };

        this.mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        // Act
        var result = await this.userService.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess); // Always returns success to prevent user enumeration
        Assert.True(result.Value);
        this.mockEmailService.Verify(x => x.SendResetPasswordToUserAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ResetPasswordAsync Tests

    [Fact]
    public async Task ResetPasswordAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Token = "reset-token",
            Password = "newpassword"
        };
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = true
        };

        this.mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.ResetPasswordAsync(user, "reset-token", "newpassword"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await this.userService.ResetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userService.ResetPasswordAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task ResetPasswordAsync_WithNonExistingUser_ReturnsSuccessForSecurity()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Token = "reset-token",
            Password = "newpassword"
        };
        this.mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.userService.ResetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess); // Always returns success to prevent user enumeration
        Assert.True(result.Value);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInvalidToken_ReturnsSuccessForSecurity()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            Token = "invalid-token",
            Password = "newpassword"
        };
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = true
        };
        var identityError = new IdentityError { Description = "Invalid token" };

        this.mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.ResetPasswordAsync(user, "invalid-token", "newpassword"))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act
        var result = await this.userService.ResetPasswordAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess); // Always returns success to prevent user enumeration
        Assert.True(result.Value);
    }

    #endregion

    #region ConfirmEmailAsync Tests

    [Fact]
    public async Task ConfirmEmailAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new ConfirmEmailRequest
        {
            Email = "test@example.com",
            Token = "confirmation-token"
        };
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = false
        };

        this.mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.ConfirmEmailAsync(user, "confirmation-token"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await this.userService.ConfirmEmailAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userService.ConfirmEmailAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithNonExistingUser_ReturnsFailure()
    {
        // Arrange
        var request = new ConfirmEmailRequest
        {
            Email = "test@example.com",
            Token = "confirmation-token"
        };
        this.mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.userService.ConfirmEmailAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Equal("Unable to confirm email", result.ErrorMessage);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var request = new ConfirmEmailRequest
        {
            Email = "test@example.com",
            Token = "invalid-token"
        };
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            EmailConfirmed = false
        };
        var identityError = new IdentityError { Description = "Invalid token" };

        this.mockUserManager.Setup(x => x.FindByEmailAsync("test@example.com"))
            .ReturnsAsync(user);
        this.mockUserManager.Setup(x => x.ConfirmEmailAsync(user, "invalid-token"))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act
        var result = await this.userService.ConfirmEmailAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);
        Assert.Contains("Invalid token", result.ErrorMessage);
    }

    #endregion
}
