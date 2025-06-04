using System.Security.Claims;
using DerpCode.API.Constants;
using DerpCode.API.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace DerpCode.API.Tests.Extensions;

/// <summary>
/// Tests for the UtilityExtensions class
/// </summary>
public class UtilityExtensionsTests
{
    #region TryApply Tests

    [Fact]
    public void TryApply_WithNullPatchDocument_ThrowsArgumentNullException()
    {
        // Arrange
        JsonPatchDocument<TestModel>? patchDoc = null;
        var target = new TestModel();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            patchDoc!.TryApply(target, out _));

        Assert.Equal("patchDoc", exception.ParamName);
    }

    [Fact]
    public void TryApply_WithValidPatch_ReturnsTrue()
    {
        // Arrange
        var patchDoc = new JsonPatchDocument<TestModel>();
        patchDoc.Replace(x => x.Name, "NewName");

        var target = new TestModel { Name = "OldName" };

        // Act
        var result = patchDoc.TryApply(target, out var error);

        // Assert
        Assert.True(result);
        Assert.Empty(error);
        Assert.Equal("NewName", target.Name);
    }

    [Fact]
    public void TryApply_WithInvalidPatch_ReturnsFalse()
    {
        // Arrange
        var patchDoc = new JsonPatchDocument<TestModel>();
        // Create an invalid operation manually
        patchDoc.Operations.Add(new Operation<TestModel>("replace", "/invalidProperty", null, "value"));

        var target = new TestModel();

        // Act
        var result = patchDoc.TryApply(target, out var error);

        // Assert
        Assert.False(result);
        Assert.NotEmpty(error);
    }

    #endregion

    #region TryGetUserId Tests

    [Fact]
    public void TryGetUserId_WithNullPrincipal_ThrowsArgumentNullException()
    {
        // Arrange
        ClaimsPrincipal? principal = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            principal!.TryGetUserId(out _));

        Assert.Equal("principal", exception.ParamName);
    }

    [Fact]
    public void TryGetUserId_WithNoNameIdentifierClaim_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Email, "test@example.com")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetUserId(out var userId);

        // Assert
        Assert.False(result);
        Assert.Null(userId);
    }

    [Fact]
    public void TryGetUserId_WithValidNameIdentifierClaim_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Name, "testuser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetUserId(out var userId);

        // Assert
        Assert.True(result);
        Assert.Equal(123, userId);
    }

    [Fact]
    public void TryGetUserId_WithInvalidNameIdentifierClaim_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "not-a-number"),
            new(ClaimTypes.Name, "testuser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetUserId(out var userId);

        // Assert
        Assert.False(result);
        Assert.Null(userId);
    }

    [Fact]
    public void TryGetUserId_WithZeroNameIdentifierClaim_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "0"),
            new(ClaimTypes.Name, "testuser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetUserId(out var userId);

        // Assert
        Assert.True(result);
        Assert.Equal(0, userId);
    }

    [Fact]
    public void TryGetUserId_WithNegativeNameIdentifierClaim_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "-1"),
            new(ClaimTypes.Name, "testuser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetUserId(out var userId);

        // Assert
        Assert.True(result);
        Assert.Equal(-1, userId);
    }

    #endregion

    #region TryGetUserEmail Tests

    [Fact]
    public void TryGetUserEmail_WithNullPrincipal_ThrowsArgumentNullException()
    {
        // Arrange
        ClaimsPrincipal? principal = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            principal!.TryGetUserEmail(out _));

        Assert.Equal("principal", exception.ParamName);
    }

    [Fact]
    public void TryGetUserEmail_WithNoEmailClaim_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Name, "testuser")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetUserEmail(out var email);

        // Assert
        Assert.False(result);
        Assert.Null(email);
    }

    [Fact]
    public void TryGetUserEmail_WithValidEmailClaim_ReturnsTrue()
    {
        // Arrange
        var expectedEmail = "test@example.com";
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Email, expectedEmail)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetUserEmail(out var email);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedEmail, email);
    }

    [Fact]
    public void TryGetUserEmail_WithEmptyEmailClaim_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Email, string.Empty)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetUserEmail(out var email);

        // Assert
        Assert.True(result);
        Assert.Equal(string.Empty, email);
    }

    #endregion

    #region TryGetEmailVerified Tests

    [Fact]
    public void TryGetEmailVerified_WithNullPrincipal_ThrowsArgumentNullException()
    {
        // Arrange
        ClaimsPrincipal? principal = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            principal!.TryGetEmailVerified(out _));

        Assert.Equal("principal", exception.ParamName);
    }

    [Fact]
    public void TryGetEmailVerified_WithNoEmailVerifiedClaim_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Email, "test@example.com")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetEmailVerified(out var emailVerified);

        // Assert
        Assert.False(result);
        Assert.Null(emailVerified);
    }

    [Fact]
    public void TryGetEmailVerified_WithTrueEmailVerifiedClaim_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(AppClaimTypes.EmailVerified, "true")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetEmailVerified(out var emailVerified);

        // Assert
        Assert.True(result);
        Assert.True(emailVerified);
    }

    [Fact]
    public void TryGetEmailVerified_WithFalseEmailVerifiedClaim_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(AppClaimTypes.EmailVerified, "false")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetEmailVerified(out var emailVerified);

        // Assert
        Assert.True(result);
        Assert.False(emailVerified);
    }

    [Fact]
    public void TryGetEmailVerified_WithInvalidEmailVerifiedClaim_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(AppClaimTypes.EmailVerified, "not-a-boolean")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetEmailVerified(out var emailVerified);

        // Assert
        Assert.False(result);
        Assert.Null(emailVerified);
    }

    [Theory]
    [InlineData("True")]
    [InlineData("TRUE")]
    [InlineData("False")]
    [InlineData("FALSE")]
    public void TryGetEmailVerified_WithVariousCasing_ReturnsTrue(string value)
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(AppClaimTypes.EmailVerified, value)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.TryGetEmailVerified(out var emailVerified);

        // Assert
        Assert.True(result);
        Assert.Equal(bool.Parse(value), emailVerified);
    }

    #endregion

    #region IsAdmin Tests

    [Fact]
    public void IsAdmin_WithNullPrincipal_ThrowsArgumentNullException()
    {
        // Arrange
        ClaimsPrincipal? principal = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            principal!.IsAdmin());

        Assert.Equal("principal", exception.ParamName);
    }

    [Fact]
    public void IsAdmin_WithNoRoleClaims_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Email, "test@example.com")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.IsAdmin();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAdmin_WithAdminRole_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Role, UserRoleName.Admin)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.IsAdmin();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAdmin_WithUserRole_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Role, UserRoleName.User)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.IsAdmin();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAdmin_WithMultipleRolesIncludingAdmin_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Role, UserRoleName.User),
            new(ClaimTypes.Role, UserRoleName.Admin)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = principal.IsAdmin();

        // Assert
        Assert.True(result);
    }

    #endregion

    #region MapPatchDocument Tests

    [Fact]
    public void MapPatchDocument_WithNullSourceDocument_ThrowsArgumentNullException()
    {
        // Arrange
        JsonPatchDocument<TestModel>? sourceDoc = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            sourceDoc!.MapPatchDocument<TestModel, TestDestinationModel>());

        Assert.Equal("sourceDoc", exception.ParamName);
    }

    [Fact]
    public void MapPatchDocument_WithEmptyDocument_ReturnsEmptyDocument()
    {
        // Arrange
        var sourceDoc = new JsonPatchDocument<TestModel>();

        // Act
        var result = sourceDoc.MapPatchDocument<TestModel, TestDestinationModel>();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Operations);
    }

    [Fact]
    public void MapPatchDocument_WithSingleOperation_CopiesOperation()
    {
        // Arrange
        var sourceDoc = new JsonPatchDocument<TestModel>();
        sourceDoc.Replace(x => x.Name, "NewName");

        // Act
        var result = sourceDoc.MapPatchDocument<TestModel, TestDestinationModel>();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Operations);

        var operation = result.Operations.First();
        Assert.Equal("replace", operation.op);
        Assert.Equal("/Name", operation.path);
        Assert.Equal("NewName", operation.value);
    }

    [Fact]
    public void MapPatchDocument_WithMultipleOperations_CopiesAllOperations()
    {
        // Arrange
        var sourceDoc = new JsonPatchDocument<TestModel>();
        sourceDoc.Replace(x => x.Name, "NewName");
        sourceDoc.Replace(x => x.Value, 42);
        sourceDoc.Add(x => x.Tags, "tag1");

        // Act
        var result = sourceDoc.MapPatchDocument<TestModel, TestDestinationModel>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Operations.Count);
    }

    [Fact]
    public void MapPatchDocument_WithPathMapper_MapsPathsCorrectly()
    {
        // Arrange
        var sourceDoc = new JsonPatchDocument<TestModel>();
        sourceDoc.Replace(x => x.Name, "NewName");
        sourceDoc.Replace(x => x.Value, 42);

        string PathMapper(string path)
        {
            return path switch
            {
                "/Name" => "/DisplayName",
                "/Value" => "/Amount",
                _ => path
            };
        }

        // Act
        var result = sourceDoc.MapPatchDocument<TestModel, TestDestinationModel>(PathMapper);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Operations.Count);

        var nameOperation = result.Operations.First(op => op.path == "/DisplayName");
        Assert.Equal("replace", nameOperation.op);
        Assert.Equal("NewName", nameOperation.value);

        var valueOperation = result.Operations.First(op => op.path == "/Amount");
        Assert.Equal("replace", valueOperation.op);
        Assert.Equal(42, valueOperation.value);
    }

    [Fact]
    public void MapPatchDocument_WithMoveOperation_MapsFromPathCorrectly()
    {
        // Arrange
        var sourceDoc = new JsonPatchDocument<TestModel>();
        // Create a move operation manually since the strongly-typed Move method doesn't work between incompatible types
        sourceDoc.Operations.Add(new Operation<TestModel>("move", "/Value", "/Name"));

        string PathMapper(string path)
        {
            return path switch
            {
                "/Name" => "/DisplayName",
                "/Value" => "/Amount",
                _ => path
            };
        }

        // Act
        var result = sourceDoc.MapPatchDocument<TestModel, TestDestinationModel>(PathMapper);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Operations);

        var operation = result.Operations.First();
        Assert.Equal("move", operation.op);
        Assert.Equal("/Amount", operation.path);
        Assert.Equal("/DisplayName", operation.from);
    }

    #endregion

    #region Helper Classes

    private class TestModel
    {
        public string Name { get; set; } = string.Empty;

        public int Value { get; set; }

        public List<string> Tags { get; set; } = new();
    }

    private class TestDestinationModel
    {
        public string DisplayName { get; set; } = string.Empty;

        public int Amount { get; set; }

        public List<string> Categories { get; set; } = new();
    }

    #endregion
}