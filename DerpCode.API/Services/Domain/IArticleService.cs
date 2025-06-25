using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;

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

    /// <summary>
    /// Retrieves a paginated list of comments for a specific article
    /// </summary>
    /// <param name="searchParams">The search params.</param>
    /// <param name="cancellationToken">The cancel token</param>
    /// <returns>List of comments.</returns>
    Task<CursorPaginatedList<ArticleCommentDto, int>> GetArticleCommentsAsync(ArticleCommentQueryParameters searchParams, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an article comment by its ID
    /// </summary>
    /// <param name="id">The ID of the article comment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The article comment DTO if found, otherwise null</returns>
    Task<ArticleCommentDto?> GetArticleCommentByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new comment for an article
    /// </summary>
    /// <param name="articleId">The ID of the article to comment on</param>
    /// <param name="createCommentRequest">The request containing the comment details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created article comment result.</returns>
    Task<Result<ArticleCommentDto>> CreateCommentAsync(int articleId, CreateArticleCommentRequest createCommentRequest, CancellationToken cancellationToken);
}