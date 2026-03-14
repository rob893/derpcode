using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Services.Auth;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for progression-related queries and operations.
/// </summary>
public sealed class ProgressionQueryService : IProgressionQueryService
{
    private readonly IExperienceEventRepository experienceEventRepository;

    private readonly ICurrentUserService currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressionQueryService"/> class.
    /// </summary>
    /// <param name="experienceEventRepository">The experience event repository.</param>
    /// <param name="currentUserService">The current user service.</param>
    public ProgressionQueryService(
        IExperienceEventRepository experienceEventRepository,
        ICurrentUserService currentUserService)
    {
        this.experienceEventRepository = experienceEventRepository ?? throw new ArgumentNullException(nameof(experienceEventRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <inheritdoc />
    public async Task<Result<CursorPaginatedResponse<ExperienceEventDto, long>>> GetXpHistoryAsync(int? first, string? after, CancellationToken cancellationToken)
    {
        var userId = this.currentUserService.UserId;

        var searchParams = new CursorPaginationQueryParameters
        {
            First = first ?? 20,
            After = after,
            IncludeNodes = true
        };

        var events = await this.experienceEventRepository.SearchAsync(
            e => e.UserId == userId,
            track: false,
            cancellationToken);

        var ordered = events.OrderByDescending(e => e.CreatedAt).ToList();

        var dtos = ordered.Select(ExperienceEventDto.FromEntity).ToList();

        var response = dtos.ToCursorPaginatedResponse(
            dto => dto.Id,
            searchParams);

        return Result<CursorPaginatedResponse<ExperienceEventDto, long>>.Success(response);
    }
}
