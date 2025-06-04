using DerpCode.API.Services;

namespace DerpCode.API.Tests.Services;

public class CorrelationIdServiceTests
{
    private readonly CorrelationIdService service;

    public CorrelationIdServiceTests()
    {
        this.service = new CorrelationIdService();
    }

    [Fact]
    public void CorrelationId_WhenAccessedFirstTime_ShouldGenerateNewGuid()
    {
        // Act
        var result = this.service.CorrelationId;

        // Assert
        Assert.False(string.IsNullOrEmpty(result));
        Assert.True(Guid.TryParse(result, out _), "because it should be a valid GUID string");
    }

    [Fact]
    public void CorrelationId_WhenAccessedMultipleTimes_ShouldReturnSameValue()
    {
        // Act
        var firstCall = this.service.CorrelationId;
        var secondCall = this.service.CorrelationId;
        var thirdCall = this.service.CorrelationId;

        // Assert
        Assert.Equal(secondCall, firstCall);
        Assert.Equal(thirdCall, secondCall);
        Assert.Equal(thirdCall, firstCall);
    }

    [Fact]
    public void CorrelationId_WhenSet_ShouldReturnSetValue()
    {
        // Arrange
        var expectedValue = "custom-correlation-id";

        // Act
        this.service.CorrelationId = expectedValue;
        var result = this.service.CorrelationId;

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void CorrelationId_WhenSetToNull_ShouldGenerateNewGuidOnNextAccess()
    {
        // Arrange
        var originalValue = this.service.CorrelationId; // Trigger initial generation

        // Act
        this.service.CorrelationId = null!; // Suppress null warning since we're testing the behavior
        var result = this.service.CorrelationId;

        // Assert
        Assert.False(string.IsNullOrEmpty(result));
        Assert.NotEqual(originalValue, result);
        Assert.True(Guid.TryParse(result, out _));
    }

    [Fact]
    public void CorrelationId_WhenSetToEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var emptyValue = string.Empty;

        // Act
        this.service.CorrelationId = emptyValue;
        var result = this.service.CorrelationId;

        // Assert
        Assert.Equal(emptyValue, result);
    }

    [Fact]
    public void CorrelationId_WhenSetToWhitespace_ShouldReturnWhitespace()
    {
        // Arrange
        var whitespaceValue = "   ";

        // Act
        this.service.CorrelationId = whitespaceValue;
        var result = this.service.CorrelationId;

        // Assert
        Assert.Equal(whitespaceValue, result);
    }

    [Fact]
    public void CorrelationId_MultipleInstances_ShouldHaveDifferentIds()
    {
        // Arrange
        var service1 = new CorrelationIdService();
        var service2 = new CorrelationIdService();

        // Act
        var id1 = service1.CorrelationId;
        var id2 = service2.CorrelationId;

        // Assert
        Assert.False(string.IsNullOrEmpty(id1));
        Assert.False(string.IsNullOrEmpty(id2));
        Assert.NotEqual(id2, id1);
        Assert.True(Guid.TryParse(id1, out _));
        Assert.True(Guid.TryParse(id2, out _));
    }

    [Fact]
    public void CorrelationId_WhenSetAfterGeneration_ShouldOverrideGeneratedValue()
    {
        // Arrange
        var generatedValue = this.service.CorrelationId; // Trigger generation
        var customValue = "override-value";

        // Act
        this.service.CorrelationId = customValue;
        var result = this.service.CorrelationId;

        // Assert
        Assert.Equal(customValue, result);
        Assert.NotEqual(generatedValue, result);
    }

    [Fact]
    public void CorrelationId_WhenSetToAnotherGuid_ShouldReturnThatGuid()
    {
        // Arrange
        var customGuid = Guid.NewGuid().ToString();

        // Act
        this.service.CorrelationId = customGuid;
        var result = this.service.CorrelationId;

        // Assert
        Assert.Equal(customGuid, result);
        Assert.True(Guid.TryParse(result, out var parsedGuid));
        Assert.Equal(customGuid, parsedGuid.ToString());
    }

    [Fact]
    public void CorrelationId_GeneratedValue_ShouldBeValidGuidFormat()
    {
        // Act
        var result = this.service.CorrelationId;

        // Assert
        Assert.False(string.IsNullOrEmpty(result));
        Assert.Equal(36, result.Length);
        Assert.Contains("-", result);
        Assert.True(Guid.TryParse(result, out var guid));
        Assert.NotEqual(Guid.Empty, guid);
    }
}