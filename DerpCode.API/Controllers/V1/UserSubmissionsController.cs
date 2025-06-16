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
        var submissionResult = await this.userSubmissionService.GetUserSubmissionsAsync(userId, searchParams, this.HttpContext.RequestAborted);

        if (!submissionResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(submissionResult);
        }

        var submissions = submissionResult.ValueOrThrow;

        var paginatedResponse = submissions.ToCursorPaginatedResponse(
            e => e.Id,
            id => id.ConvertToBase64Url(),
            id => id.ConvertToLongFromBase64Url(),
            searchParams);

        return this.Ok(paginatedResponse);
    }
}