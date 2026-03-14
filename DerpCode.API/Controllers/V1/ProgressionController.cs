using System;
using System.Threading.Tasks;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Controllers.V1;

/// <summary>
/// Controller for user progression and XP history.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/progression")]
[ApiVersion("1.0")]
public sealed class ProgressionController : ServiceControllerBase
{
    private readonly IProgressionQueryService progressionQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressionController"/> class.
    /// </summary>
    /// <param name="correlationIdService">The correlation ID service.</param>
    /// <param name="progressionQueryService">The progression query service.</param>
    public ProgressionController(ICorrelationIdService correlationIdService, IProgressionQueryService progressionQueryService)
        : base(correlationIdService)
    {
        this.progressionQueryService = progressionQueryService ?? throw new ArgumentNullException(nameof(progressionQueryService));
    }

    /// <summary>
    /// Gets the XP event history for the current user.
    /// </summary>
    /// <param name="first">Number of items to return (default 20).</param>
    /// <param name="after">Cursor for forward pagination.</param>
    /// <returns>A paginated list of XP events.</returns>
    /// <response code="200">Returns the XP history.</response>
    [HttpGet("xp-history", Name = nameof(GetXpHistoryAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<ExperienceEventDto, long>>> GetXpHistoryAsync(
        [FromQuery] int? first,
        [FromQuery] string? after)
    {
        var result = await this.progressionQueryService.GetXpHistoryAsync(first, after, this.HttpContext.RequestAborted);

        if (!result.IsSuccess)
        {
            return this.HandleServiceFailureResult(result);
        }

        return this.Ok(result.ValueOrThrow);
    }
}
