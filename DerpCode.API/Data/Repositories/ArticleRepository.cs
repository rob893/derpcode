using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
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

        var list = await query.ToCursorPaginatedListAsync(
            item => item.Id,
            this.ConvertIdToBase64,
            this.ConvertBase64ToIdType,
            searchParams,
            cancellationToken);

        return list;
    }
}