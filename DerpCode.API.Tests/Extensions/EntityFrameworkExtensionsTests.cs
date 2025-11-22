using DerpCode.API.Extensions;
using DerpCode.API.Models;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Tests.Extensions;

public class EntityFrameworkExtensionsTests
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

    /// <summary>
    /// Test DbContext for in-memory testing.
    /// </summary>
    private sealed class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; } = default!;

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
            });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test database context with sample data.
    /// </summary>
    private static async Task<TestDbContext> CreateTestContextAsync()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new TestDbContext(options);

        // Add test data
        var testData = new List<TestEntity>
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

        context.TestEntities.AddRange(testData);
        await context.SaveChangesAsync();

        return context;
    }

    #endregion

    #region Basic Cursor Pagination Tests

    [Fact]
    public async Task ToCursorPaginatedListAsync_WithFirst_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var queryParameters = new CursorPaginationQueryParameters { First = 3, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(queryParameters);

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
    public async Task ToCursorPaginatedListAsync_WithLast_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var queryParameters = new CursorPaginationQueryParameters { Last = 3, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(queryParameters);

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
    public async Task ToCursorPaginatedListAsync_WithAfterCursor_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var afterCursor = 3.ConvertToBase64UrlEncodedString();
        var queryParameters = new CursorPaginationQueryParameters
        {
            First = 3,
            After = afterCursor,
            IncludeTotal = false
        };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(queryParameters);

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
    public async Task ToCursorPaginatedListAsync_WithBeforeCursor_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var beforeCursor = 8.ConvertToBase64UrlEncodedString();
        var queryParameters = new CursorPaginationQueryParameters
        {
            Last = 3,
            Before = beforeCursor,
            IncludeTotal = false
        };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(queryParameters);

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
    public async Task ToCursorPaginatedListAsync_WithOrderByAscending_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var queryParameters = new CursorPaginationQueryParameters { First = 4, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(
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
    public async Task ToCursorPaginatedListAsync_WithOrderByAscendingString_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var queryParameters = new CursorPaginationQueryParameters { First = 4, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(
            entity => entity.Id,
            entity => entity.Name,
            CursorConverters.CreateCompositeKeyConverterStringInt(),
            CursorConverters.CreateCompositeCursorConverterStringInt(),
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

        Assert.Equal("Alpha", items[0].Name);
        Assert.Equal("Beta", items[1].Name);
        Assert.Equal("Delta", items[2].Name);
        Assert.Equal("Epsilon", items[3].Name);
    }

    [Fact]
    public async Task ToCursorPaginatedListAsync_WithOrderByDescending_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var queryParameters = new CursorPaginationQueryParameters { First = 4, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(
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
    public async Task ToCursorPaginatedListAsync_WithDateTimeOrdering_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var queryParameters = new CursorPaginationQueryParameters { First = 3, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(
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
    public async Task ToCursorPaginatedListAsync_WithCompositeAfterCursor_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var afterCursor = CursorConverters.CreateCompositeKeyConverterIntInt()((1, 3)); // Priority 1, Id 3
        var queryParameters = new CursorPaginationQueryParameters
        {
            First = 3,
            After = afterCursor,
            IncludeTotal = true
        };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(
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
    public async Task ToCursorPaginatedListAsync_WithConvenienceOverload_ReturnsCorrectResults()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();
        var queryParameters = new CursorPaginationQueryParameters { First = 3, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(
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
    public async Task ToCursorPaginatedListAsync_WithBothFirstAndLast_ThrowsNotSupportedException()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            context.TestEntities.ToCursorPaginatedListAsync(
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
    public async Task ToCursorPaginatedListAsync_WithNegativeFirst_ThrowsArgumentException()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            context.TestEntities.ToCursorPaginatedListAsync(
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
    public async Task ToCursorPaginatedListAsync_WithNegativeLast_ThrowsArgumentException()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            context.TestEntities.ToCursorPaginatedListAsync(
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
    public async Task ToCursorPaginatedListAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IQueryable<TestEntity> nullSource = null!;
        var queryParameters = new CursorPaginationQueryParameters { First = 3 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullSource.ToCursorPaginatedListAsync(queryParameters));
    }

    [Fact]
    public async Task ToCursorPaginatedListAsync_WithNullQueryParameters_ThrowsArgumentNullException()
    {
        // Arrange
        await using var context = await CreateTestContextAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            context.TestEntities.ToCursorPaginatedListAsync(null!));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ToCursorPaginatedListAsync_WithEmptyDataset_ReturnsEmptyResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new TestDbContext(options);
        var queryParameters = new CursorPaginationQueryParameters { First = 10, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(queryParameters);

        // Assert
        Assert.Equal(0, result.PageCount);
        Assert.Equal(0, result.TotalCount);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
        Assert.Null(result.StartCursor);
        Assert.Null(result.EndCursor);
    }

    [Fact]
    public async Task ToCursorPaginatedListAsync_WithSingleItem_ReturnsCorrectResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new TestDbContext(options);
        context.TestEntities.Add(new TestEntity { Id = 1, Name = "Single", Priority = 1 });
        await context.SaveChangesAsync();

        var queryParameters = new CursorPaginationQueryParameters { First = 10, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(queryParameters);

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
    public async Task ToCursorPaginatedListAsync_WithDuplicateOrderValues_MaintainsConsistentOrdering()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new TestDbContext(options);

        // Add entities with same priority
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "First", Priority = 1 },
            new TestEntity { Id = 2, Name = "Second", Priority = 1 },
            new TestEntity { Id = 3, Name = "Third", Priority = 1 },
            new TestEntity { Id = 4, Name = "Fourth", Priority = 2 }
        };
        context.TestEntities.AddRange(entities);
        await context.SaveChangesAsync();

        var queryParameters = new CursorPaginationQueryParameters { First = 2, IncludeTotal = true };

        // Act
        var result = await context.TestEntities.ToCursorPaginatedListAsync(
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

    #endregion
}
