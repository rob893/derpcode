
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public sealed class ProblemRepository(DataContext context) : Repository<Problem, CursorPaginationQueryParameters>(context), IProblemRepository
{
}