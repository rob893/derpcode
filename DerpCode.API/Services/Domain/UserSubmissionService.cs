using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Services.Auth;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing user submission-related business logic
/// </summary>
public sealed class UserSubmissionService : IUserSubmissionService
{
    private readonly IProblemSubmissionRepository problemSubmissionRepository;

    private readonly ICurrentUserService currentUserService;

    private readonly ILogger<UserSubmissionService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSubmissionService"/> class
    /// </summary>
    /// <param name="problemSubmissionRepository">The problem submission repository</param>
    /// <param name="currentUserService">The current user service</param>
    /// <param name="logger">The logger</param>
    public UserSubmissionService(IProblemSubmissionRepository problemSubmissionRepository, ICurrentUserService currentUserService, ILogger<UserSubmissionService> logger)
    {
        this.problemSubmissionRepository = problemSubmissionRepository ?? throw new ArgumentNullException(nameof(problemSubmissionRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<CursorPaginatedList<ProblemSubmissionDto, long>>> GetUserSubmissionsAsync(int userId, UserSubmissionQueryParameters searchParams, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(searchParams);

        if (!this.currentUserService.IsAdmin && this.currentUserService.UserId != userId)
        {
            this.logger.LogWarning("User {UserId} attempted to access submissions for user {TargetUserId} without permission.", this.currentUserService.UserId, userId);
            return Result<CursorPaginatedList<ProblemSubmissionDto, long>>.Failure(DomainErrorType.Forbidden, "You do not have permission to view these submissions.");
        }

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
            .Select(x => ProblemSubmissionDto.FromEntity(x, this.currentUserService.IsAdmin || this.currentUserService.IsPremiumUser))
            .ToList();

        return Result<CursorPaginatedList<ProblemSubmissionDto, long>>.Success(new CursorPaginatedList<ProblemSubmissionDto, long>(mapped, submissions.HasNextPage, submissions.HasPreviousPage, submissions.StartCursor, submissions.EndCursor, submissions.TotalCount));
    }
}
