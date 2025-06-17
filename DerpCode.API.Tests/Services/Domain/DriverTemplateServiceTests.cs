using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Services.Domain;
using Moq;
using Xunit;

namespace DerpCode.API.Tests.Services.Domain;

/// <summary>
/// Unit tests for the <see cref="DriverTemplateService"/> class
/// </summary>
public sealed class DriverTemplateServiceTests
{
    private readonly Mock<IDriverTemplateRepository> mockDriverTemplateRepository;

    private readonly DriverTemplateService driverTemplateService;

    public DriverTemplateServiceTests()
    {
        this.mockDriverTemplateRepository = new Mock<IDriverTemplateRepository>();

        this.driverTemplateService = new DriverTemplateService(this.mockDriverTemplateRepository.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDriverTemplateRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new DriverTemplateService(null!));

        Assert.Equal("driverTemplateRepository", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new DriverTemplateService(this.mockDriverTemplateRepository.Object);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region GetDriverTemplatesAsync Tests

    [Fact]
    public async Task GetDriverTemplatesAsync_WithNullSearchParams_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this.driverTemplateService.GetDriverTemplatesAsync(null!, CancellationToken.None));

        Assert.Equal("searchParams", exception.ParamName);
    }

    [Fact]
    public async Task GetDriverTemplatesAsync_WithValidParams_ReturnsDriverTemplates()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 10 };
        var driverTemplates = new List<DriverTemplate> { CreateTestDriverTemplate() };
        var pagedList = new CursorPaginatedList<DriverTemplate, int>(driverTemplates, false, false, "1", "1", 1);

        this.mockDriverTemplateRepository
            .Setup(x => x.SearchAsync(searchParams, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.driverTemplateService.GetDriverTemplatesAsync(searchParams, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
        Assert.Equal(LanguageType.CSharp, result.First().Language);
        Assert.Equal("test template code", result.First().Template);
        Assert.Equal("test ui template", result.First().UITemplate);
    }

    [Fact]
    public async Task GetDriverTemplatesAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 10 };
        var emptyList = new List<DriverTemplate>();
        var pagedList = new CursorPaginatedList<DriverTemplate, int>(emptyList, false, false, null, null, 0);

        this.mockDriverTemplateRepository
            .Setup(x => x.SearchAsync(searchParams, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.driverTemplateService.GetDriverTemplatesAsync(searchParams, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetDriverTemplatesAsync_WithMultipleTemplates_ReturnsAllTemplates()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 10 };
        var driverTemplates = new List<DriverTemplate>
        {
            CreateTestDriverTemplate(1, LanguageType.CSharp),
            CreateTestDriverTemplate(2, LanguageType.JavaScript),
            CreateTestDriverTemplate(3, LanguageType.TypeScript)
        };
        var pagedList = new CursorPaginatedList<DriverTemplate, int>(driverTemplates, false, false, "1", "3", 3);

        this.mockDriverTemplateRepository
            .Setup(x => x.SearchAsync(searchParams, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.driverTemplateService.GetDriverTemplatesAsync(searchParams, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.PageCount);
        Assert.Equal(3, result.TotalCount);
        Assert.Contains(result, x => x.Language == LanguageType.CSharp);
        Assert.Contains(result, x => x.Language == LanguageType.JavaScript);
        Assert.Contains(result, x => x.Language == LanguageType.TypeScript);
    }

    [Fact]
    public async Task GetDriverTemplatesAsync_WithPaginationInfo_ReturnsPaginationDetails()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 2 };
        var driverTemplates = new List<DriverTemplate>
        {
            CreateTestDriverTemplate(1, LanguageType.CSharp),
            CreateTestDriverTemplate(2, LanguageType.JavaScript)
        };
        var pagedList = new CursorPaginatedList<DriverTemplate, int>(driverTemplates, true, false, "1", "2", 5);

        this.mockDriverTemplateRepository
            .Setup(x => x.SearchAsync(searchParams, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedList);

        // Act
        var result = await this.driverTemplateService.GetDriverTemplatesAsync(searchParams, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PageCount);
        Assert.Equal(5, result.TotalCount);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
        Assert.Equal("1", result.StartCursor);
        Assert.Equal("2", result.EndCursor);
    }

    [Fact]
    public async Task GetDriverTemplatesAsync_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var searchParams = new CursorPaginationQueryParameters { First = 5 };
        var cancellationToken = new CancellationToken();
        var emptyList = new List<DriverTemplate>();
        var pagedList = new CursorPaginatedList<DriverTemplate, int>(emptyList, false, false, null, null, 0);

        this.mockDriverTemplateRepository
            .Setup(x => x.SearchAsync(searchParams, false, cancellationToken))
            .ReturnsAsync(pagedList);

        // Act
        await this.driverTemplateService.GetDriverTemplatesAsync(searchParams, cancellationToken);

        // Assert
        this.mockDriverTemplateRepository.Verify(
            x => x.SearchAsync(searchParams, false, cancellationToken),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static DriverTemplate CreateTestDriverTemplate(int id = 1, LanguageType language = LanguageType.CSharp)
    {
        return new DriverTemplate
        {
            Id = id,
            Language = language,
            Template = "test template code",
            UITemplate = "test ui template"
        };
    }

    #endregion
}
