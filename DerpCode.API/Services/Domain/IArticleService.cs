using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing articles business logic
/// </summary>
public interface IArticleService
{
    /// <summary>
    /// Retrieves a paginated list of articles
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated list of articles</returns>
    Task<CursorPaginatedList<ArticleDto, int>> GetArticlesAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an article by its ID
    /// </summary>
    /// <param name="id">The ID of the article</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The article DTO if found, otherwise null</returns>
    Task<ArticleDto?> GetArticleByIdAsync(int id, CancellationToken cancellationToken);
}