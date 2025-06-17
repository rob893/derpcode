using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing driver template-related business logic
/// </summary>
public interface IDriverTemplateService
{
    /// <summary>
    /// Retrieves a paginated list of driver templates
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated list of driver templates</returns>
    Task<CursorPaginatedList<DriverTemplateDto, int>> GetDriverTemplatesAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken);
}
