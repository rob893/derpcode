using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Text;
using DerpCode.API.Constants;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Settings;
using DerpCode.API.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DerpCode.API.Tests.Services;

public sealed class JwtTokenServiceTests
{
    private readonly Mock<IUserRepository> mockUserRepository;

    private readonly Mock<IOptions<AuthenticationSettings>> mockAuthOptions;

    private readonly AuthenticationSettings authSettings;

    private readonly JwtTokenService jwtTokenService;

    public JwtTokenServiceTests()
    {
        this.mockUserRepository = new Mock<IUserRepository>();
        this.mockAuthOptions = new Mock<IOptions<AuthenticationSettings>>();

        this.authSettings = new AuthenticationSettings
        {
            APISecret = "test-secret-key-that-is-at-least-512-bits-long-for-hmac-sha512-signing-and-validation-purposes-in-jwt-tokens",
            TokenAudience = "test-audience",
            TokenIssuer = "test-issuer",
            TokenExpirationTimeInMinutes = 60,
            RefreshTokenExpirationTimeInMinutes = 10080 // 7 days
        };

        this.mockAuthOptions.Setup(x => x.Value).Returns(this.authSettings);
        this.jwtTokenService = new JwtTokenService(this.mockUserRepository.Object, this.mockAuthOptions.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUserRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new JwtTokenService(null!, this.mockAuthOptions.Object));

        Assert.Equal("userRepository", exception!.ParamName);
    }

    [Fact]
    public void Constructor_WithNullAuthOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new JwtTokenService(this.mockUserRepository.Object, null!));

        Assert.Equal("authSettings", exception!.ParamName);
    }

    [Fact]
    public void Constructor_WithNullAuthOptionsValue_ThrowsArgumentNullException()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<AuthenticationSettings>>();
        mockOptions.Setup(x => x.Value).Returns((AuthenticationSettings)null!);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new JwtTokenService(this.mockUserRepository.Object, mockOptions.Object));

        Assert.Equal("authSettings", exception!.ParamName);
    }

    #endregion

    #region GenerateJwtTokenForUser Tests

    [Fact]
    public void GenerateJwtTokenForUser_WithNullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            this.jwtTokenService.GenerateJwtTokenForUser(null!));

        Assert.Equal("user", exception!.ParamName);
    }

    [Fact]
    public void GenerateJwtTokenForUser_WithShortAPISecret_ThrowsArgumentException()
    {
        // Arrange
        var shortSecretSettings = new AuthenticationSettings
        {
            APISecret = "short",
            TokenAudience = "test-audience",
            TokenIssuer = "test-issuer",
            TokenExpirationTimeInMinutes = 60,
            RefreshTokenExpirationTimeInMinutes = 10080
        };

        this.mockAuthOptions.Setup(x => x.Value).Returns(shortSecretSettings);
        var serviceWithShortSecret = new JwtTokenService(this.mockUserRepository.Object, this.mockAuthOptions.Object);

        var user = CreateTestUser();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            serviceWithShortSecret.GenerateJwtTokenForUser(user));

        Assert.Equal("API Secret must be longer", exception!.Message);
    }

    [Fact]
    public void GenerateJwtTokenForUser_WithValidUser_ReturnsValidToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = this.jwtTokenService.GenerateJwtTokenForUser(user);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = CreateTokenValidationParameters();

        // This should not throw an exception
        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

        Assert.NotNull(validatedToken);
        Assert.NotNull(principal);
    }

    [Fact]
    public void GenerateJwtTokenForUser_ValidatesTokenClaims()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = this.jwtTokenService.GenerateJwtTokenForUser(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var claims = jsonToken.Claims.ToList();

        // JWT tokens use short claim names and lowercase boolean values
        Assert.Contains(claims, c => c.Type == "nameid" && c.Value == "1");
        Assert.Contains(claims, c => c.Type == "unique_name" && c.Value == user.UserName);
        Assert.Contains(claims, c => c.Type == "email" && c.Value == user.Email);
        Assert.Contains(claims, c => c.Type == "email_verified" && c.Value == "true");  // JWT uses lowercase
        Assert.Contains(claims, c => c.Type == "role" && c.Value == UserRoleName.User);
    }

    [Fact]
    public void GenerateJwtTokenForUser_WithNullUserName_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser();
        user.UserName = null;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            this.jwtTokenService.GenerateJwtTokenForUser(user));

        Assert.Equal("UserName cannot be null", exception!.Message);
    }

    [Fact]
    public void GenerateJwtTokenForUser_WithNullEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser();
        user.Email = null;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            this.jwtTokenService.GenerateJwtTokenForUser(user));

        Assert.Equal("Email cannot be null", exception!.Message);
    }

    #endregion

    #region GenerateAndSaveRefreshTokenForUserAsync Tests

    [Fact]
    public async Task GenerateAndSaveRefreshTokenForUserAsync_WithNullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(null!, "deviceId", CancellationToken.None));
    }

    [Fact]
    public async Task GenerateAndSaveRefreshTokenForUserAsync_WithNullDeviceId_ThrowsArgumentNullException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, null!, CancellationToken.None));
    }

    [Fact]
    public async Task GenerateAndSaveRefreshTokenForUserAsync_WithEmptyDeviceId_ThrowsArgumentNullException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task GenerateAndSaveRefreshTokenForUserAsync_WithWhitespaceDeviceId_ThrowsArgumentNullException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, "   ", CancellationToken.None));
    }

    [Fact]
    public async Task GenerateAndSaveRefreshTokenForUserAsync_WithValidParameters_ReturnsToken()
    {
        // Arrange
        var user = CreateTestUser();
        var deviceId = "test-device-id";

        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, deviceId, CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrEmpty(result));
        Assert.Single(user.RefreshTokens);
        Assert.Equal(deviceId, user.RefreshTokens.First().DeviceId);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region RevokeRefreshTokenForDeviceAsync Tests

    [Fact]
    public async Task RevokeRefreshTokenForDeviceAsync_WithNullDeviceId_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.jwtTokenService.RevokeRefreshTokenForDeviceAsync(1, null!, CancellationToken.None));
    }

    [Fact]
    public async Task RevokeRefreshTokenForDeviceAsync_WithEmptyDeviceId_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.jwtTokenService.RevokeRefreshTokenForDeviceAsync(1, string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task RevokeRefreshTokenForDeviceAsync_WithValidDeviceId_RevokesTokens()
    {
        // Arrange
        var userId = 1;
        var deviceId = "test-device-id";
        var user = CreateTestUser();
        user.RefreshTokens = new List<RefreshToken>
        {
            new() { TokenHash = "token1-hash", DeviceId = deviceId },
            new() { TokenHash = "token2-hash", DeviceId = deviceId }
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this.jwtTokenService.RevokeRefreshTokenForDeviceAsync(userId, deviceId, CancellationToken.None);

        // Assert
        Assert.DoesNotContain(user.RefreshTokens, token => token.DeviceId == deviceId);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region RevokeAllRefreshTokensForUserAsync Tests

    [Fact]
    public async Task RevokeAllRefreshTokensForUserAsync_WithUserIdZero_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await this.jwtTokenService.RevokeAllRefreshTokensForUserAsync(0, CancellationToken.None));
    }

    [Fact]
    public async Task RevokeAllRefreshTokensForUserAsync_WithNonExistentUser_ThrowsArgumentException()
    {
        // Arrange
        var userId = 999;

        this.mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await this.jwtTokenService.RevokeAllRefreshTokensForUserAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task RevokeAllRefreshTokensForUserAsync_WithValidUserId_RevokesAllTokens()
    {
        // Arrange
        var userId = 1;
        var user = CreateTestUser();
        user.RefreshTokens = new List<RefreshToken>
        {
            new() { TokenHash = "token1-hash" },
            new() { TokenHash = "token2-hash" },
            new() { TokenHash = "token3-hash" }
        };

        this.mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<Expression<Func<User, object>>[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        this.mockUserRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this.jwtTokenService.RevokeAllRefreshTokensForUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.Empty(user.RefreshTokens);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static User CreateTestUser()
    {
        return new User
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true,
            UserRoles = new List<UserRole>
            {
                new()
                {
                    Role = new Role { Name = UserRoleName.User }
                }
            },
            RefreshTokens = new List<RefreshToken>()
        };
    }

    private TokenValidationParameters CreateTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.authSettings.APISecret)),
            ValidateIssuer = true,
            ValidIssuer = this.authSettings.TokenIssuer,
            ValidateAudience = true,
            ValidAudience = this.authSettings.TokenAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    #endregion
}