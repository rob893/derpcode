using System;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V1;

[Route("api/v{version:apiVersion}/users/{userId}/submissions")]
[ApiVersion("1")]
[ApiController]
public sealed class UserSubmissionsController : ServiceControllerBase
{
    private readonly IProblemSubmissionRepository problemSubmissionRepository;

    private readonly ILogger<UserSubmissionsController> logger;

    public UserSubmissionsController(
        IProblemSubmissionRepository problemSubmissionRepository,
        ILogger<UserSubmissionsController> logger,
        ICorrelationIdService correlationIdService)
        : base(correlationIdService)
    {
        this.problemSubmissionRepository = problemSubmissionRepository ?? throw new ArgumentNullException(nameof(problemSubmissionRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet(Name = nameof(GetProblemSubmissionsForUserAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CursorPaginatedResponse<ProblemSubmissionDto>>> GetProblemSubmissionsForUserAsync([FromRoute] int userId, [FromQuery] UserSubmissionQueryParameters searchParams)
    {
        if (searchParams == null)
        {
            return this.BadRequest("Search parameters cannot be null.");
        }

        if (!this.User.TryGetUserId(out var currentUserId))
        {
            return this.Unauthorized("You must be logged in to view submissions.");
        }

        if (!this.User.IsAdmin() && currentUserId != userId)
        {
            this.logger.LogWarning("User {UserId} attempted to access submissions for user {TargetUserId} without permission.", currentUserId, userId);
            return this.Forbidden("You can only see your own submissions.");
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

        var submissions = await this.problemSubmissionRepository.SearchAsync(problemSearchParams, track: false, this.HttpContext.RequestAborted);
        var paginatedResponse = submissions.Select(ProblemSubmissionDto.FromEntity).ToCursorPaginatedResponse(searchParams);

        return this.Ok(paginatedResponse);
    }
}