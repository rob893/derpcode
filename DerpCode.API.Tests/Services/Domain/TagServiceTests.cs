using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Services.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace DerpCode.API.Tests.Services.Domain;

/// <summary>
/// Unit tests for the <see cref="TagService"/> class
/// </summary>
public sealed class TagServiceTests : IDisposable
{
    private readonly Mock<ITagRepository> mockTagRepository;

    private readonly IMemoryCache memoryCache;

    private readonly TagService tagService;

    public TagServiceTests()
    {
        this.mockTagRepository = new Mock<ITagRepository>();
        this.memoryCache = new MemoryCache(new MemoryCacheOptions());

        this.tagService = new TagService(this.mockTagRepository.Object, this.memoryCache);
    }

    public void Dispose()
    {
        this.memoryCache.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullTagRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TagService(null!, this.memoryCache));

        Assert.Equal("tagRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullMemoryCache_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TagService(this.mockTagRepository.Object, null!));

        Assert.Equal("cache", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new TagService(this.mockTagRepository.Object, this.memoryCache);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region GetTagsAsync Tests

    [Fact]
    public async Task GetTagsAsync_WithNullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.tagService.GetTagsAsync(null!, CancellationToken.None));

        Assert.Equal("searchParams", exception.ParamName);
    }

    [Fact]
    public async Task GetTagsAsync_WithValidParams_ReturnsTags()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 10, IncludeTotal = true };
        var tags = new List<Tag> { CreateTestTag(1, "Array"), CreateTestTag(2, "String") };

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await this.tagService.GetTagsAsync(searchParams, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PageCount);
        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result, x => x.Name == "Array");
        Assert.Contains(result, x => x.Name == "String");
    }

    [Fact]
    public async Task GetTagsAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 10, IncludeTotal = true };
        var emptyList = new List<Tag>();

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await this.tagService.GetTagsAsync(searchParams, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetTagsAsync_UsesCache_OnSecondCall()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 10 };
        var tags = new List<Tag> { CreateTestTag(1, "Array") };

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        await this.tagService.GetTagsAsync(searchParams, CancellationToken.None);
        await this.tagService.GetTagsAsync(searchParams, CancellationToken.None);

        // Assert - Repository should only be called once, second call uses cache
        this.mockTagRepository.Verify(
            x => x.SearchAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTagsAsync_ReturnsSortedTags()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 10 };
        var tags = new List<Tag>
        {
            CreateTestTag(1, "Zebra"),
            CreateTestTag(2, "Array"),
            CreateTestTag(3, "String")
        };

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await this.tagService.GetTagsAsync(searchParams, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Equal("Array", resultList[0].Name);
        Assert.Equal("String", resultList[1].Name);
        Assert.Equal("Zebra", resultList[2].Name);
    }

    #endregion

    #region GetTagByIdAsync Tests

    [Fact]
    public async Task GetTagByIdAsync_WithValidId_ReturnsTag()
    {
        // Arrange
        var tags = new List<Tag> { CreateTestTag(1, "Array"), CreateTestTag(2, "String") };

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await this.tagService.GetTagByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Array", result.Name);
    }

    [Fact]
    public async Task GetTagByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var tags = new List<Tag> { CreateTestTag(1, "Array"), CreateTestTag(2, "String") };

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await this.tagService.GetTagByIdAsync(999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTagByIdAsync_UsesCache_OnSecondCall()
    {
        // Arrange
        var tags = new List<Tag> { CreateTestTag(1, "Array") };

        this.mockTagRepository
            .Setup(x => x.SearchAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        await this.tagService.GetTagByIdAsync(1, CancellationToken.None);
        await this.tagService.GetTagByIdAsync(1, CancellationToken.None);

        // Assert - Repository should only be called once, second call uses cache
        this.mockTagRepository.Verify(
            x => x.SearchAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static Tag CreateTestTag(int id, string name)
    {
        return new Tag
        {
            Id = id,
            Name = name,
            Problems = [],
            Articles = []
        };
    }

    #endregion
}
