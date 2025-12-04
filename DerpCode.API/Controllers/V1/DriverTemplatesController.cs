using System;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/driverTemplates")]
[ApiVersion("1.0")]
public sealed class DriverTemplatesController : ServiceControllerBase
{
    private readonly IDriverTemplateService driverTemplateService;

    public DriverTemplatesController(ICorrelationIdService correlationIdService, IDriverTemplateService driverTemplateService) : base(correlationIdService)
    {
        this.driverTemplateService = driverTemplateService ?? throw new ArgumentNullException(nameof(driverTemplateService));
    }

    /// <summary>
    /// Gets a paginated list of driver templates. Admin access required.
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters.</param>
    /// <returns>A paginated list of driver templates.</returns>
    /// <response code="200">Returns the paginated list of driver templates.</response>
    [HttpGet(Name = nameof(GetDriverTemplatesAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<DriverTemplateDto>>> GetDriverTemplatesAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var templates = await this.driverTemplateService.GetDriverTemplatesAsync(searchParams, this.HttpContext.RequestAborted);
        var response = templates.ToCursorPaginatedResponse(searchParams);

        return this.Ok(response);
    }
}