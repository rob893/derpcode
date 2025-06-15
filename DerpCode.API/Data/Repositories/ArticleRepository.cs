using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public sealed class ArticleRepository(DataContext context) : Repository<Article, CursorPaginationQueryParameters>(context), IArticleRepository
{ }