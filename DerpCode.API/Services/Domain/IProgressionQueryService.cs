using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Responses.Pagination;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for progression-related queries and operations.
/// </summary>
public interface IProgressionQueryService
{
    /// <summary>
    /// Gets the XP event history for the current user.
    /// </summary>
    /// <param name="first">Number of items to return.</param>
    /// <param name="after">Cursor for forward pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of experience events.</returns>
    Task<Result<CursorPaginatedResponse<ExperienceEventDto, long>>> GetXpHistoryAsync(int? first, string? after, CancellationToken cancellationToken);
}
