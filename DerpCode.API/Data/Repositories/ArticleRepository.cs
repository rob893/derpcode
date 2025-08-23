using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Extensions;
using DerpCode.API.Models;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Data.Repositories;

public sealed class ArticleRepository(DataContext context) : Repository<Article, CursorPaginationQueryParameters>(context), IArticleRepository
{
    public async Task<CursorPaginatedList<ArticleComment, int>> SearchArticleCommentsAsync(ArticleCommentQueryParameters searchParams, bool track = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchParams);

        IQueryable<ArticleComment> query = this.Context.Set<ArticleComment>();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        if (searchParams.ArticleId.HasValue)
        {
            query = query.Where(comment => comment.ArticleId == searchParams.ArticleId.Value);
        }

        if (searchParams.ParentCommentId.HasValue)
        {
            query = query.Where(comment => comment.ParentCommentId == searchParams.ParentCommentId.Value);
        }

        if (searchParams.QuotedCommentId.HasValue)
        {
            query = query.Where(comment => comment.QuotedCommentId == searchParams.QuotedCommentId.Value);
        }

        query = query.Include(c => c.User);

        var list = searchParams.OrderBy switch
        {
            ArticleCommentOrderBy.MostRecent => await query.ToCursorPaginatedListAsync(
                item => item.Id,
                item => item.CreatedAt,
                CursorConverters.CreateCompositeKeyConverterDateTimeOffsetInt(),
                CursorConverters.CreateCompositeCursorConverterDateTimeOffsetInt(),
                searchParams,
                searchParams.OrderByDirection == OrderByDirection.Ascending,
                cancellationToken),
            ArticleCommentOrderBy.HighestRated => await query.ToCursorPaginatedListAsync(
                item => item.Id,
                item => item.UpVotes,
                CursorConverters.CreateCompositeKeyConverterIntInt(),
                CursorConverters.CreateCompositeCursorConverterIntInt(),
                searchParams,
                searchParams.OrderByDirection == OrderByDirection.Ascending,
                cancellationToken),
            _ => throw new InvalidOperationException("Invalid OrderBy value")
        };

        return list;
    }

    public async Task<ArticleComment?> GetArticleCommentByIdAsync(int id, bool track = true, CancellationToken cancellationToken = default)
    {
        IQueryable<ArticleComment> query = this.Context.Set<ArticleComment>();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        query = query.Include(c => c.User);

        return await query.FirstOrDefaultAsync(comment => comment.Id == id, cancellationToken);
    }
}