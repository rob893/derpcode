using System;
using System.Linq;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public sealed class ProblemSubmissionRepository : Repository<ProblemSubmission, long, ProblemSubmissionQueryParameters>, IProblemSubmissionRepository
{
    public ProblemSubmissionRepository(DataContext context) : base(
            context,
            Id => Id.ConvertToBase64UrlEncodedString(),
            str =>
            {
                try
                {
                    return str.ConvertToLongFromBase64UrlEncodedString();
                }
                catch
                {
                    throw new ArgumentException($"{str} is not a valid base 64 encoded int64.");
                }
            }
        )
    { }

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