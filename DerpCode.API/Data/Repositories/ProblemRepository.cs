
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Data.Repositories;

public sealed class ProblemRepository(DataContext context) : Repository<Problem, CursorPaginationQueryParameters>(context), IProblemRepository
{
    public async Task<IReadOnlyList<PersonalizedProblemLimitedDto>> GetPersonalizedProblemListAsync(int userId, CancellationToken cancellationToken)
    {
        var problems = await this.Context.Problems
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Select(p => new PersonalizedProblemLimitedDto
            {
                Id = p.Id,
                IsPublished = p.IsPublished,
                Name = p.Name,
                Difficulty = p.Difficulty,
                IsFavorite = p.FavoritedByUsers.Any(f => f.UserId == userId),
                Tags = p.Tags
                    .Select(t => new TagDto
                    {
                        Id = t.Id,
                        Name = t.Name
                    }).ToList(),
                LastPassedSubmissionDate = p.ProblemSubmissions
                    .Where(submission => submission.UserId == userId && submission.Pass)
                    .Max(submission => submission.CreatedAt),
                LastSubmissionDate = p.ProblemSubmissions
                    .Where(submission => submission.UserId == userId)
                    .Max(submission => submission.CreatedAt),
            }
            )
            .ToListAsync(cancellationToken);

        return problems;
    }

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