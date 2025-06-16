using System;
using System.Threading.Tasks;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V1;

[Route("api/v{version:apiVersion}/users/{userId}/submissions")]
[ApiVersion("1")]
[ApiController]
public sealed class UserSubmissionsController : ServiceControllerBase
{
    private readonly IUserSubmissionService userSubmissionService;

    private readonly ILogger<UserSubmissionsController> logger;

    public UserSubmissionsController(
        IUserSubmissionService userSubmissionService,
        ILogger<UserSubmissionsController> logger,
        ICorrelationIdService correlationIdService)
        : base(correlationIdService)
    {
        this.userSubmissionService = userSubmissionService ?? throw new ArgumentNullException(nameof(userSubmissionService));
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

        var submissions = await this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, this.HttpContext.RequestAborted);
        var paginatedResponse = submissions.ToCursorPaginatedResponse(
            e => e.Id,
            id => id.ConvertToBase64Url(),
            id => id.ConvertToLongFromBase64Url(),
            searchParams);

        return this.Ok(paginatedResponse);
    }
}