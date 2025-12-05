using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing tags business logic
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Gets a paginated list of tags.
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated list of tags</returns>
    Task<CursorPaginatedList<TagDto, int>> GetTagsAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a tag by its ID.
    /// </summary>
    /// <param name="id">The ID of the tag</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tag DTO if found, otherwise null</returns>
    Task<TagDto?> GetTagByIdAsync(int id, CancellationToken cancellationToken);
}
