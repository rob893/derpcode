using System;
using System.Linq;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public sealed class ProblemSubmissionRepository(DataContext context) : Repository<ProblemSubmission, ProblemSubmissionQueryParameters>(context), IProblemSubmissionRepository
{
    protected override IQueryable<ProblemSubmission> AddWhereClauses(IQueryable<ProblemSubmission> query, ProblemSubmissionQueryParameters searchParams)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(searchParams);

        if (searchParams.ProblemId != null)
        {
            query = query.Where(submission => submission.ProblemId == searchParams.ProblemId);
        }

        if (searchParams.UserId != null)
        {
            query = query.Where(submission => submission.UserId == searchParams.UserId);
        }

        return query;
    }
}