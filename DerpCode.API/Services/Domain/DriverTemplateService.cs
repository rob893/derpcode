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
/// Service for managing driver template-related business logic
/// </summary>
public sealed class DriverTemplateService : IDriverTemplateService
{
    private readonly IDriverTemplateRepository driverTemplateRepository;

    public DriverTemplateService(IDriverTemplateRepository driverTemplateRepository)
    {
        this.driverTemplateRepository = driverTemplateRepository ?? throw new ArgumentNullException(nameof(driverTemplateRepository));
    }

    /// <inheritdoc />
    public async Task<CursorPaginatedList<DriverTemplateDto, int>> GetDriverTemplatesAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(searchParams);

        var pagedList = await this.driverTemplateRepository.SearchAsync(searchParams, track: false, cancellationToken);
        var mapped = pagedList
            .Select(DriverTemplateDto.FromEntity)
            .ToList();

        return new CursorPaginatedList<DriverTemplateDto, int>(mapped, pagedList.HasNextPage, pagedList.HasPreviousPage, pagedList.StartCursor, pagedList.EndCursor, pagedList.TotalCount);
    }
}
