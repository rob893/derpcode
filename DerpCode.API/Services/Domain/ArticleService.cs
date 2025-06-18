using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing driver template-related business logic
/// </summary>
public sealed class ArticleService : IArticleService
{
    private readonly IArticleRepository articleRepository;

    public ArticleService(IArticleRepository articleRepository)
    {
        this.articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
    }

    /// <inheritdoc />
    public async Task<CursorPaginatedList<ArticleDto, int>> GetArticlesAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(searchParams);

        var pagedList = await this.articleRepository.SearchAsync(searchParams, track: false, cancellationToken);
        var mapped = pagedList
            .Select(ArticleDto.FromEntity)
            .ToList();

        return new CursorPaginatedList<ArticleDto, int>(mapped, pagedList.HasNextPage, pagedList.HasPreviousPage, pagedList.StartCursor, pagedList.EndCursor, pagedList.TotalCount);
    }

    /// <inheritdoc />
    public async Task<ArticleDto?> GetArticleByIdAsync(int id, CancellationToken cancellationToken)
    {
        var article = await this.articleRepository.GetByIdAsync(id, track: false, cancellationToken);

        if (article == null)
        {
            return null;
        }

        return ArticleDto.FromEntity(article);
    }
}