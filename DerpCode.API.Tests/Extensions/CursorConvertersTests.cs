using DerpCode.API.Utilities;

namespace DerpCode.API.Tests.Extensions;

/// <summary>
/// Tests for the CursorConverters class.
/// </summary>
public class CursorConvertersTests
{
    /// <summary>
    /// Tests that the generic factory methods work correctly for string/int combination.
    /// </summary>
    [Fact]
    public void CreateCompositeKeyConverter_StringInt_Generic_ShouldReturnCorrectConverter()
    {
        // Arrange
        var keyConverter = CursorConverters.CreateCompositeKeyConverter<string, int>();
        var cursorConverter = CursorConverters.CreateCompositeCursorConverter<string, int>();
        var originalKey = ("test", 123);

        // Act
        var cursor = keyConverter(originalKey);
        var restoredKey = cursorConverter(cursor);

        // Assert
        Assert.Equal(originalKey, restoredKey);
    }

    /// <summary>
    /// Tests that the generic factory methods work correctly for DateTime/long combination.
    /// </summary>
    [Fact]
    public void CreateCompositeKeyConverter_DateTimeLong_Generic_ShouldReturnCorrectConverter()
    {
        // Arrange
        var keyConverter = CursorConverters.CreateCompositeKeyConverter<DateTime, long>();
        var cursorConverter = CursorConverters.CreateCompositeCursorConverter<DateTime, long>();
        var testDate = new DateTime(2023, 5, 15, 10, 30, 45);
        var originalKey = (testDate, 456L);

        // Act
        var cursor = keyConverter(originalKey);
        var restoredKey = cursorConverter(cursor);

        // Assert
        Assert.Equal(originalKey, restoredKey);
    }

    /// <summary>
    /// Tests that the specific converter methods work correctly for DateTimeOffset/int combination.
    /// </summary>
    [Fact]
    public void CreateCompositeKeyConverterDateTimeOffsetInt_ShouldWorkCorrectly()
    {
        // Arrange
        var keyConverter = CursorConverters.CreateCompositeKeyConverterDateTimeOffsetInt();
        var cursorConverter = CursorConverters.CreateCompositeCursorConverterDateTimeOffsetInt();
        var testDateOffset = new DateTimeOffset(2023, 5, 15, 10, 30, 45, TimeSpan.FromHours(5));
        var originalKey = (testDateOffset, 789);

        // Act
        var cursor = keyConverter(originalKey);
        var restoredKey = cursorConverter(cursor);

        // Assert
        Assert.Equal(originalKey, restoredKey);
    }

    /// <summary>
    /// Tests that the generic factory methods throw NotSupportedException for unsupported type combinations.
    /// </summary>
    [Fact]
    public void CreateCompositeKeyConverter_UnsupportedType_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => CursorConverters.CreateCompositeKeyConverter<decimal, int>());
        Assert.Throws<NotSupportedException>(() => CursorConverters.CreateCompositeCursorConverter<decimal, int>());
    }

    /// <summary>
    /// Tests that cursor converters throw ArgumentException for invalid cursor formats.
    /// </summary>
    [Fact]
    public void CreateCompositeCursorConverter_InvalidFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var cursorConverter = CursorConverters.CreateCompositeCursorConverterStringInt();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cursorConverter("invalid"));
        Assert.Throws<ArgumentException>(() => cursorConverter("too|many|parts|here"));
    }

    /// <summary>
    /// Tests that DateTimeOffset cursor converters handle the three-part format correctly.
    /// </summary>
    [Fact]
    public void CreateCompositeCursorConverterDateTimeOffset_InvalidFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var cursorConverter = CursorConverters.CreateCompositeCursorConverterDateTimeOffsetInt();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cursorConverter("only|two"));
        Assert.Throws<ArgumentException>(() => cursorConverter("too|many|parts|here|extra"));
    }
}
