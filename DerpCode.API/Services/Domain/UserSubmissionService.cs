using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing user submission-related business logic
/// </summary>
public sealed class UserSubmissionService : IUserSubmissionService
{
    private readonly IProblemSubmissionRepository problemSubmissionRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSubmissionService"/> class
    /// </summary>
    /// <param name="problemSubmissionRepository">The problem submission repository</param>
    public UserSubmissionService(IProblemSubmissionRepository problemSubmissionRepository)
    {
        this.problemSubmissionRepository = problemSubmissionRepository ?? throw new ArgumentNullException(nameof(problemSubmissionRepository));
    }

    /// <inheritdoc />
    public async Task<CursorPaginatedList<ProblemSubmissionDto, long>> GetUserSubmissionsAsync(int userId, UserSubmissionQueryParameters searchParams, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(searchParams);

        var problemSearchParams = new ProblemSubmissionQueryParameters
        {
            ProblemId = searchParams.ProblemId,
            UserId = userId,
            After = searchParams.After,
            Before = searchParams.Before,
            First = searchParams.First,
            Last = searchParams.Last,
            IncludeTotal = searchParams.IncludeTotal,
            IncludeNodes = searchParams.IncludeNodes,
            IncludeEdges = searchParams.IncludeEdges
        };

        var submissions = await this.problemSubmissionRepository.SearchAsync(problemSearchParams, track: false, cancellationToken);
        var mapped = submissions
            .Select(ProblemSubmissionDto.FromEntity)
            .ToList();

        return new CursorPaginatedList<ProblemSubmissionDto, long>(mapped, submissions.HasNextPage, submissions.HasPreviousPage, submissions.StartCursor, submissions.EndCursor, submissions.TotalCount);
    }
}
