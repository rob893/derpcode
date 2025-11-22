using DerpCode.API.Extensions;
using DerpCode.API.Models;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Utilities;

namespace DerpCode.API.Tests.Extensions;

public class CollectionExtensionsTests
{
    #region Test Entity Models

    /// <summary>
    /// Test entity for pagination tests.
    /// </summary>
    private sealed class TestEntity : IIdentifiable<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public int Priority { get; set; }

        public double Score { get; set; }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates test data for pagination tests.
    /// </summary>
    private static List<TestEntity> CreateTestData()
    {
        return new List<TestEntity>
        {
            new() { Id = 1, Name = "Alpha", CreatedAt = new DateTime(2023, 1, 1), Priority = 1, Score = 95.5 },
            new() { Id = 2, Name = "Beta", CreatedAt = new DateTime(2023, 1, 2), Priority = 2, Score = 87.2 },
            new() { Id = 3, Name = "Gamma", CreatedAt = new DateTime(2023, 1, 3), Priority = 1, Score = 92.1 },
            new() { Id = 4, Name = "Delta", CreatedAt = new DateTime(2023, 1, 4), Priority = 3, Score = 78.9 },
            new() { Id = 5, Name = "Epsilon", CreatedAt = new DateTime(2023, 1, 5), Priority = 2, Score = 89.7 },
            new() { Id = 6, Name = "Zeta", CreatedAt = new DateTime(2023, 1, 6), Priority = 1, Score = 91.3 },
            new() { Id = 7, Name = "Eta", CreatedAt = new DateTime(2023, 1, 7), Priority = 3, Score = 85.4 },
            new() { Id = 8, Name = "Theta", CreatedAt = new DateTime(2023, 1, 8), Priority = 2, Score = 88.6 },
            new() { Id = 9, Name = "Iota", CreatedAt = new DateTime(2023, 1, 9), Priority = 1, Score = 93.8 },
            new() { Id = 10, Name = "Kappa", CreatedAt = new DateTime(2023, 1, 10), Priority = 3, Score = 76.2 }
        };
    }

    #endregion

    #region Basic Cursor Pagination Tests

    [Fact]
    public void ToCursorPaginatedList_WithFirst_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 3, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(queryParameters);

        // Assert
        Assert.Equal(3, result.PageCount);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);

        var items = result.ToList();
        Assert.Equal(1, items[0].Id);
        Assert.Equal(2, items[1].Id);
        Assert.Equal(3, items[2].Id);
    }

    [Fact]
    public void ToCursorPaginatedList_WithLast_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { Last = 3, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(queryParameters);

        // Assert
        Assert.Equal(3, result.PageCount);
        Assert.Equal(10, result.TotalCount);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);

        var items = result.ToList();
        Assert.Equal(8, items[0].Id);
        Assert.Equal(9, items[1].Id);
        Assert.Equal(10, items[2].Id);
    }

    [Fact]
    public void ToCursorPaginatedList_WithAfterCursor_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var afterCursor = 3.ConvertToBase64UrlEncodedString();
        var queryParameters = new CursorPaginationQueryParameters
        {
            First = 3,
            After = afterCursor,
            IncludeTotal = false
        };

        // Act
        var result = testData.ToCursorPaginatedList(queryParameters);

        // Assert
        Assert.Equal(3, result.PageCount);
        Assert.Null(result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);

        var items = result.ToList();
        Assert.Equal(4, items[0].Id);
        Assert.Equal(5, items[1].Id);
        Assert.Equal(6, items[2].Id);
    }

    [Fact]
    public void ToCursorPaginatedList_WithBeforeCursor_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var beforeCursor = 8.ConvertToBase64UrlEncodedString();
        var queryParameters = new CursorPaginationQueryParameters
        {
            Last = 3,
            Before = beforeCursor,
            IncludeTotal = false
        };

        // Act
        var result = testData.ToCursorPaginatedList(queryParameters);

        // Assert
        Assert.Equal(3, result.PageCount);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);

        var items = result.ToList();
        Assert.Equal(5, items[0].Id);
        Assert.Equal(6, items[1].Id);
        Assert.Equal(7, items[2].Id);
    }

    #endregion

    #region Composite Cursor Pagination Tests

    [Fact]
    public void ToCursorPaginatedList_WithOrderByAscending_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 4, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Id,
            entity => entity.Priority,
            CursorConverters.CreateCompositeKeyConverterIntInt(),
            CursorConverters.CreateCompositeCursorConverterIntInt(),
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending: true);

        // Assert
        Assert.Equal(4, result.PageCount);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);

        var items = result.ToList();
        // Should be ordered by Priority (1,1,1,1) then by Id (1,3,6,9)
        Assert.Equal(1, items[0].Id); // Priority 1, Id 1
        Assert.Equal(3, items[1].Id); // Priority 1, Id 3
        Assert.Equal(6, items[2].Id); // Priority 1, Id 6
        Assert.Equal(9, items[3].Id); // Priority 1, Id 9
    }

    [Fact]
    public void ToCursorPaginatedList_WithOrderByAscendingDateTime_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 4, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Id,
            entity => entity.CreatedAt,
            CursorConverters.CreateCompositeKeyConverterDateTimeInt(),
            CursorConverters.CreateCompositeCursorConverterDateTimeInt(),
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending: true);

        // Assert
        Assert.Equal(4, result.PageCount);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);

        var items = result.ToList();

        Assert.Equal(1, items[0].Id);
        Assert.Equal(2, items[1].Id);
        Assert.Equal(3, items[2].Id);
        Assert.Equal(4, items[3].Id);
    }

    [Fact]
    public void ToCursorPaginatedList_WithOrderByDescending_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 4, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Id,
            entity => entity.Priority,
            CursorConverters.CreateCompositeKeyConverterIntInt(),
            CursorConverters.CreateCompositeCursorConverterIntInt(),
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending: false);

        // Assert
        Assert.Equal(4, result.PageCount);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);

        var items = result.ToList();
        // Should be ordered by Priority desc (3,3,3,2) then by Id asc (4,7,10,2)
        Assert.Equal(4, items[0].Id);  // Priority 3, Id 4
        Assert.Equal(7, items[1].Id);  // Priority 3, Id 7
        Assert.Equal(10, items[2].Id); // Priority 3, Id 10
        Assert.Equal(2, items[3].Id);  // Priority 2, Id 2
    }

    [Fact]
    public void ToCursorPaginatedList_WithDateTimeOrdering_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 3, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Id,
            entity => entity.CreatedAt,
            composite => $"{composite.OrderValue:yyyy-MM-dd}|{composite.Key.ConvertToBase64UrlEncodedString()}",
            cursor =>
            {
                var parts = cursor.Split('|');
                return (DateTime.ParseExact(parts[0], "yyyy-MM-dd", null), parts[1].ConvertToInt32FromBase64UrlEncodedString());
            },
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending: true);

        // Assert
        Assert.Equal(3, result.PageCount);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);

        var items = result.ToList();
        Assert.Equal(1, items[0].Id);
        Assert.Equal(2, items[1].Id);
        Assert.Equal(3, items[2].Id);

        // Check ordering by date
        Assert.True(items[0].CreatedAt <= items[1].CreatedAt);
        Assert.True(items[1].CreatedAt <= items[2].CreatedAt);
    }

    [Fact]
    public void ToCursorPaginatedList_WithCompositeAfterCursor_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var afterCursor = CursorConverters.CreateCompositeKeyConverterIntInt()((1, 3)); // Priority 1, Id 3
        var queryParameters = new CursorPaginationQueryParameters
        {
            First = 3,
            After = afterCursor,
            IncludeTotal = true
        };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Id,
            entity => entity.Priority,
            CursorConverters.CreateCompositeKeyConverterIntInt(),
            CursorConverters.CreateCompositeCursorConverterIntInt(),
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending: true);

        // Assert
        Assert.Equal(3, result.PageCount);
        Assert.Equal(8, result.TotalCount); // Only 8 items after Priority 1, Id 3
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);

        var items = result.ToList();
        // Should start after Priority 1, Id 3, so next should be Priority 1, Id 6
        Assert.Equal(6, items[0].Id); // Priority 1, Id 6
        Assert.Equal(9, items[1].Id); // Priority 1, Id 9
        Assert.Equal(2, items[2].Id); // Priority 2, Id 2
    }

    [Fact]
    public void ToCursorPaginatedList_WithConvenienceOverload_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 3, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Priority,
            queryParameters,
            ascending: true);

        // Assert
        Assert.Equal(3, result.PageCount);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);

        var items = result.ToList();
        Assert.Equal(1, items[0].Id); // Priority 1, Id 1
        Assert.Equal(3, items[1].Id); // Priority 1, Id 3
        Assert.Equal(6, items[2].Id); // Priority 1, Id 6
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void ToCursorPaginatedList_WithBothFirstAndLast_ThrowsNotSupportedException()
    {
        // Arrange
        var testData = CreateTestData();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            testData.ToCursorPaginatedList(
                entity => entity.Id,
                entity => entity.Priority,
                CursorConverters.CreateCompositeKeyConverterIntInt(),
                CursorConverters.CreateCompositeCursorConverterIntInt(),
                first: 3,
                last: 3,
                afterCursor: null,
                beforeCursor: null,
                includeTotal: true));
    }

    [Fact]
    public void ToCursorPaginatedList_WithNegativeFirst_ThrowsArgumentException()
    {
        // Arrange
        var testData = CreateTestData();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            testData.ToCursorPaginatedList(
                entity => entity.Id,
                entity => entity.Priority,
                CursorConverters.CreateCompositeKeyConverterIntInt(),
                CursorConverters.CreateCompositeCursorConverterIntInt(),
                first: -1,
                last: null,
                afterCursor: null,
                beforeCursor: null,
                includeTotal: true));
    }

    [Fact]
    public void ToCursorPaginatedList_WithNegativeLast_ThrowsArgumentException()
    {
        // Arrange
        var testData = CreateTestData();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            testData.ToCursorPaginatedList(
                entity => entity.Id,
                entity => entity.Priority,
                CursorConverters.CreateCompositeKeyConverterIntInt(),
                CursorConverters.CreateCompositeCursorConverterIntInt(),
                first: null,
                last: -1,
                afterCursor: null,
                beforeCursor: null,
                includeTotal: true));
    }

    [Fact]
    public void ToCursorPaginatedList_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<TestEntity> nullSource = null!;
        var queryParameters = new CursorPaginationQueryParameters { First = 3 };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            nullSource.ToCursorPaginatedList(queryParameters));
    }

    [Fact]
    public void ToCursorPaginatedList_WithNullQueryParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var testData = CreateTestData();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            testData.ToCursorPaginatedList(null!));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ToCursorPaginatedList_WithEmptyDataset_ReturnsEmptyResult()
    {
        // Arrange
        var emptyData = new List<TestEntity>();
        var queryParameters = new CursorPaginationQueryParameters { First = 10, IncludeTotal = true };

        // Act
        var result = emptyData.ToCursorPaginatedList(queryParameters);

        // Assert
        Assert.Equal(0, result.PageCount);
        Assert.Equal(0, result.TotalCount);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
        Assert.Null(result.StartCursor);
        Assert.Null(result.EndCursor);
    }

    [Fact]
    public void ToCursorPaginatedList_WithSingleItem_ReturnsCorrectResult()
    {
        // Arrange
        var singleItemData = new List<TestEntity>
        {
            new() { Id = 1, Name = "Single", Priority = 1 }
        };
        var queryParameters = new CursorPaginationQueryParameters { First = 10, IncludeTotal = true };

        // Act
        var result = singleItemData.ToCursorPaginatedList(queryParameters);

        // Assert
        Assert.Equal(1, result.PageCount);
        Assert.Equal(1, result.TotalCount);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
        Assert.NotNull(result.StartCursor);
        Assert.NotNull(result.EndCursor);
        Assert.Equal(result.StartCursor, result.EndCursor);
    }

    [Fact]
    public void ToCursorPaginatedList_WithDuplicateOrderValues_MaintainsConsistentOrdering()
    {
        // Arrange
        var testData = new List<TestEntity>
        {
            new() { Id = 1, Name = "First", Priority = 1 },
            new() { Id = 2, Name = "Second", Priority = 1 },
            new() { Id = 3, Name = "Third", Priority = 1 },
            new() { Id = 4, Name = "Fourth", Priority = 2 }
        };
        var queryParameters = new CursorPaginationQueryParameters { First = 2, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Id,
            entity => entity.Priority,
            CursorConverters.CreateCompositeKeyConverterIntInt(),
            CursorConverters.CreateCompositeCursorConverterIntInt(),
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending: true);

        // Assert
        Assert.Equal(2, result.PageCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);

        var items = result.ToList();
        Assert.Equal(1, items[0].Id); // Priority 1, Id 1
        Assert.Equal(2, items[1].Id); // Priority 1, Id 2
    }

    [Fact]
    public void ToCursorPaginatedList_WithStringOrdering_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 3, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Id,
            entity => entity.Name,
            composite => $"{composite.OrderValue.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}",
            cursor =>
            {
                var parts = cursor.Split('|');
                return (parts[0].ConvertToStringFromBase64UrlEncodedString(), parts[1].ConvertToInt32FromBase64UrlEncodedString());
            },
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending: true);

        // Assert
        Assert.Equal(3, result.PageCount);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);

        var items = result.ToList();
        // Should be ordered by Name alphabetically: Alpha, Beta, Delta
        Assert.Equal("Alpha", items[0].Name);
        Assert.Equal("Beta", items[1].Name);
        Assert.Equal("Delta", items[2].Name);
    }

    [Fact]
    public void ToCursorPaginatedList_WithDoubleOrdering_ReturnsCorrectResults()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 3, IncludeTotal = true };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Id,
            entity => entity.Score,
            composite => $"{composite.OrderValue}|{composite.Key.ConvertToBase64UrlEncodedString()}",
            cursor =>
            {
                var parts = cursor.Split('|');
                return (double.Parse(parts[0]), parts[1].ConvertToInt32FromBase64UrlEncodedString());
            },
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending: false); // Descending to get highest scores first

        // Assert
        Assert.Equal(3, result.PageCount);
        Assert.Equal(10, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);

        var items = result.ToList();
        // Should be ordered by Score descending: 95.5, 93.8, 92.1
        Assert.Equal(95.5, items[0].Score);
        Assert.Equal(93.8, items[1].Score);
        Assert.Equal(92.1, items[2].Score);
    }

    #endregion

    #region Encode/Decode Tests

    [Fact]
    public void EncodeOrderValue_WithInt_ReturnsCorrectEncoding()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 1 };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.Priority,
            queryParameters);

        // Assert
        Assert.Single(result);
        Assert.NotNull(result.StartCursor);

        // The cursor should be decodable
        var parts = result.StartCursor!.Split('|');
        Assert.Equal(2, parts.Length);
        var priority = parts[0].ConvertToInt32FromBase64UrlEncodedString();
        var id = parts[1].ConvertToInt32FromBase64UrlEncodedString();
        Assert.Equal(1, priority);
        Assert.Equal(1, id);
    }

    [Fact]
    public void EncodeOrderValue_WithDateTime_ReturnsCorrectEncoding()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 1 };

        // Act
        var result = testData.ToCursorPaginatedList(
            entity => entity.CreatedAt,
            queryParameters);

        // Assert
        Assert.Single(result);
        Assert.NotNull(result.StartCursor);

        // The cursor should be decodable
        var parts = result.StartCursor!.Split('|');
        Assert.Equal(2, parts.Length);
        var dateTimeBinary = parts[0].ConvertToLongFromBase64UrlEncodedString();
        var decodedDateTime = DateTime.FromBinary(dateTimeBinary);
        var id = parts[1].ConvertToInt32FromBase64UrlEncodedString();
        Assert.Equal(new DateTime(2023, 1, 1), decodedDateTime);
        Assert.Equal(1, id);
    }

    [Fact]
    public void EncodeOrderValue_WithString_ReturnsCorrectEncoding()
    {
        // Arrange
        var testData = CreateTestData();
        var queryParameters = new CursorPaginationQueryParameters { First = 1 };

        // Act - Order by name to get Alpha first
        var result = testData.ToCursorPaginatedList(
            entity => entity.Name,
            queryParameters);

        // Assert
        Assert.Single(result);
        Assert.NotNull(result.StartCursor);

        // The cursor should be decodable
        var parts = result.StartCursor!.Split('|');
        Assert.Equal(2, parts.Length);
        var name = parts[0].ConvertToStringFromBase64UrlEncodedString();
        var id = parts[1].ConvertToInt32FromBase64UrlEncodedString();
        Assert.Equal("Alpha", name);
        Assert.Equal(1, id);
    }

    #endregion
}
