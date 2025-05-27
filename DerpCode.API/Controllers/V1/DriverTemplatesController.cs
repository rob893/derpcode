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

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/driverTemplates")]
[ApiVersion("1.0")]
public class DriverTemplatesController : ServiceControllerBase
{
    private readonly IDriverTemplateRepository driverTemplateRepository;

    public DriverTemplatesController(ICorrelationIdService correlationIdService, IDriverTemplateRepository driverTemplateRepository) : base(correlationIdService)
    {
        this.driverTemplateRepository = driverTemplateRepository ?? throw new ArgumentNullException(nameof(driverTemplateRepository));
    }

    [HttpGet(Name = nameof(GetDriverTemplatesAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<DriverTemplateDto>>> GetDriverTemplatesAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var templates = await this.driverTemplateRepository.SearchAsync(searchParams, track: false, this.HttpContext.RequestAborted);
        var response = templates.Select(DriverTemplateDto.FromEntity).ToCursorPaginatedResponse(searchParams);
        return this.Ok(response);
    }
}