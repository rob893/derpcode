
using System.Linq;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Data.Repositories;

public sealed class ProblemRepository(DataContext context) : Repository<Problem, CursorPaginationQueryParameters>(context), IProblemRepository
{
    protected override IQueryable<Problem> AddIncludes(IQueryable<Problem> query)
    {
        return query
            .Include(problem => problem.Tags)
            .Include(problem => problem.Drivers);
    }

    protected override void PostProcess(Problem entity)
    {
        if (entity == null)
        {
            return;
        }

        entity.Tags?.Sort((a, b) => a.Id.CompareTo(b.Id));
        entity.Drivers?.Sort((a, b) => a.Id.CompareTo(b.Id));
    }
}