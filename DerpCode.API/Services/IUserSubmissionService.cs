using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Services;

/// <summary>
/// Service for managing user submission-related business logic
/// </summary>
public interface IUserSubmissionService
{
    /// <summary>
    /// Retrieves a paginated list of submissions for a specific user
    /// </summary>
    /// <param name="userId">The user ID to get submissions for</param>
    /// <param name="searchParams">The search parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A result containing a paginated list of user submissions</returns>
    Task<CursorPaginatedList<ProblemSubmissionDto, long>> GetUserSubmissionsAsync(int userId, UserSubmissionQueryParameters searchParams, CancellationToken cancellationToken);
}
