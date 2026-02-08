using System.Linq.Expressions;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Tests.Services.Domain;

/// <summary>
/// Unit tests for the <see cref="UserPreferencesServices"/> class
/// </summary>
public sealed class UserPreferencesServicesTests
{
    private readonly Mock<IUserPreferencesRepository> mockUserPreferencesRepository;

    private readonly Mock<ICurrentUserService> mockCurrentUserService;

    private readonly Mock<ILogger<UserPreferencesServices>> mockLogger;

    private readonly UserPreferencesServices userPreferencesServices;

    public UserPreferencesServicesTests()
    {
        this.mockUserPreferencesRepository = new Mock<IUserPreferencesRepository>();
        this.mockCurrentUserService = new Mock<ICurrentUserService>();
        this.mockLogger = new Mock<ILogger<UserPreferencesServices>>();

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsUserLoggedIn).Returns(true);
        this.mockCurrentUserService.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);

        this.userPreferencesServices = new UserPreferencesServices(
            this.mockUserPreferencesRepository.Object,
            this.mockCurrentUserService.Object,
            this.mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserPreferencesServices(null!, this.mockCurrentUserService.Object, this.mockLogger.Object));

        Assert.Equal("userPreferencesRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullCurrentUserService_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserPreferencesServices(this.mockUserPreferencesRepository.Object, null!, this.mockLogger.Object));

        Assert.Equal("currentUserService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserPreferencesServices(this.mockUserPreferencesRepository.Object, this.mockCurrentUserService.Object, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        var service = new UserPreferencesServices(this.mockUserPreferencesRepository.Object, this.mockCurrentUserService.Object, this.mockLogger.Object);

        Assert.NotNull(service);
    }

    #endregion

    #region GetUserPreferencesAsync Tests

    [Fact]
    public async Task GetUserPreferencesAsync_WhenUserRequestsOtherUserAndNotAdmin_ReturnsForbidden()
    {
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(false);

        var result = await this.userPreferencesServices.GetUserPreferencesAsync(2, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);

        this.mockUserPreferencesRepository.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), false, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_WhenPreferencesNotFound_ReturnsNotFound()
    {
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(false);

        this.mockUserPreferencesRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPreferences?)null);

        var result = await this.userPreferencesServices.GetUserPreferencesAsync(1, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);

        this.mockUserPreferencesRepository.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_WhenPreferencesFound_ReturnsDto()
    {
        var entity = CreateTestUserPreferences(userId: 1, id: 5);
        entity.Preferences.UIPreference.PageSize = 42;

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(false);

        this.mockUserPreferencesRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await this.userPreferencesServices.GetUserPreferencesAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(5, result.Value.Id);
        Assert.Equal(1, result.Value.UserId);
        Assert.Equal(42, result.Value.Preferences.UIPreference.PageSize);
    }

    #endregion

    #region PatchPreferencesAsync Tests

    [Fact]
    public async Task PatchPreferencesAsync_WithNullPatchDocument_ThrowsArgumentNullException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.userPreferencesServices.PatchPreferencesAsync(1, 1, null!, CancellationToken.None));

        Assert.Equal("patchDocument", exception.ParamName);
    }

    [Fact]
    public async Task PatchPreferencesAsync_WhenUserRequestsOtherUserAndNotAdmin_ReturnsForbidden()
    {
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(false);

        var patchDoc = new JsonPatchDocument<PatchUserPreferencesRequest>();
        patchDoc.Replace(x => x.Preferences.UIPreference.PageSize, 10);

        var result = await this.userPreferencesServices.PatchPreferencesAsync(2, 1, patchDoc, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Forbidden, result.ErrorType);

        this.mockUserPreferencesRepository.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Never);

        this.mockUserPreferencesRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PatchPreferencesAsync_WhenPatchHasNoOperations_ReturnsValidation()
    {
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(false);

        var patchDoc = new JsonPatchDocument<PatchUserPreferencesRequest>();

        var result = await this.userPreferencesServices.PatchPreferencesAsync(1, 1, patchDoc, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);

        this.mockUserPreferencesRepository.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), true, It.IsAny<CancellationToken>()),
            Times.Never);

        this.mockUserPreferencesRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PatchPreferencesAsync_WhenPreferencesNotFound_ReturnsNotFound()
    {
        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(false);

        var patchDoc = new JsonPatchDocument<PatchUserPreferencesRequest>();
        patchDoc.Replace(x => x.Preferences.UIPreference.PageSize, 10);

        this.mockUserPreferencesRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserPreferences?)null);

        var result = await this.userPreferencesServices.PatchPreferencesAsync(1, 123, patchDoc, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.NotFound, result.ErrorType);

        this.mockUserPreferencesRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PatchPreferencesAsync_WhenPatchIsInvalid_ReturnsValidationAndDoesNotSave()
    {
        var entity = CreateTestUserPreferences(userId: 1, id: 10);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(false);

        this.mockUserPreferencesRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var patchDoc = new JsonPatchDocument<PatchUserPreferencesRequest>();
        patchDoc.Operations.Add(new Operation<PatchUserPreferencesRequest>("replace", "/preferences/uipreference/pagesize", from: null, value: "not-an-int"));

        var result = await this.userPreferencesServices.PatchPreferencesAsync(1, 10, patchDoc, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Validation, result.ErrorType);

        this.mockUserPreferencesRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PatchPreferencesAsync_WithValidPatch_UpdatesPreferencesAndSaves()
    {
        var originalLastUpdated = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var entity = CreateTestUserPreferences(userId: 1, id: 10);
        entity.LastUpdated = originalLastUpdated;

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(false);

        this.mockUserPreferencesRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        this.mockUserPreferencesRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var patchDoc = new JsonPatchDocument<PatchUserPreferencesRequest>();
        patchDoc.Replace(x => x.Preferences.UIPreference.PageSize, 25);
        patchDoc.Replace(x => x.Preferences.EditorPreference.EnableFlameEffects, false);
        patchDoc.Replace(x => x.Preferences.CodePreference.DefaultLanguage, LanguageType.Python);

        var result = await this.userPreferencesServices.PatchPreferencesAsync(1, 10, patchDoc, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(25, result.Value.Preferences.UIPreference.PageSize);
        Assert.False(result.Value.Preferences.EditorPreference.EnableFlameEffects);
        Assert.Equal(LanguageType.Python, result.Value.Preferences.CodePreference.DefaultLanguage);

        Assert.NotEqual(originalLastUpdated, result.Value.LastUpdated);
        Assert.True(result.Value.LastUpdated > originalLastUpdated);

        this.mockUserPreferencesRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PatchPreferencesAsync_WhenSaveChangesReturnsZero_ReturnsUnknown()
    {
        var entity = CreateTestUserPreferences(userId: 1, id: 10);

        this.mockCurrentUserService.Setup(x => x.UserId).Returns(1);
        this.mockCurrentUserService.Setup(x => x.IsInRole(UserRoleName.Admin)).Returns(false);

        this.mockUserPreferencesRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserPreferences, bool>>>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        this.mockUserPreferencesRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var patchDoc = new JsonPatchDocument<PatchUserPreferencesRequest>();
        patchDoc.Replace(x => x.Preferences.UIPreference.PageSize, 10);

        var result = await this.userPreferencesServices.PatchPreferencesAsync(1, 10, patchDoc, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(DomainErrorType.Unknown, result.ErrorType);
    }

    #endregion

    private static UserPreferences CreateTestUserPreferences(int userId, int id)
    {
        return new UserPreferences
        {
            Id = id,
            UserId = userId,
            User = null!,
            Preferences = new Preferences
            {
                UIPreference = new UserUIPreference
                {
                    PageSize = 5,
                    UITheme = UITheme.Dark
                },
                CodePreference = new UserCodePreference
                {
                    DefaultLanguage = LanguageType.JavaScript
                },
                EditorPreference = new UserEditorPreference
                {
                    EnableFlameEffects = true
                }
            },
            LastUpdated = DateTimeOffset.UtcNow
        };
    }
}
