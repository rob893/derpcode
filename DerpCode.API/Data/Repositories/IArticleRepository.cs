
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public interface IArticleRepository : IRepository<Article, CursorPaginationQueryParameters>
{
    Task<CursorPaginatedList<ArticleComment, int>> SearchArticleCommentsAsync(ArticleCommentQueryParameters searchParams, bool track = true, CancellationToken cancellationToken = default);

    Task<ArticleComment?> GetArticleCommentByIdAsync(int id, bool track = true, CancellationToken cancellationToken = default);
}