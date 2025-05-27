using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public sealed class TagRepository(DataContext context) : Repository<Tag, CursorPaginationQueryParameters>(context), ITagRepository
{ }