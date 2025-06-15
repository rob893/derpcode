
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
            .Include(problem => problem.ExplanationArticle)
            .ThenInclude(article => article.Tags)
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

    protected override void BeforeRemove(Problem entity)
    {
        if (entity == null)
        {
            return;
        }

        if (entity.ExplanationArticle != null)
        {
            this.Context.Articles.Remove(entity.ExplanationArticle);
        }

        if (entity.SolutionArticles != null && entity.SolutionArticles.Count > 0)
        {
            this.Context.Articles.RemoveRange(entity.SolutionArticles);
        }
    }
}