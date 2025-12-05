using System;
using System.Threading.Tasks;
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
[Route("api/v{version:apiVersion}/tags")]
[ApiVersion("1.0")]
public sealed class TagsController : ServiceControllerBase
{
    private readonly ITagService tagService;

    public TagsController(ICorrelationIdService correlationIdService, ITagService tagService)
        : base(correlationIdService)
    {
        this.tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
    }

    [AllowAnonymous]
    [HttpGet(Name = nameof(GetTagsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<TagDto>>> GetTagsAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var tags = await this.tagService.GetTagsAsync(searchParams, this.HttpContext.RequestAborted);
        var response = tags.ToCursorPaginatedResponse(searchParams);
        return this.Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{id}", Name = nameof(GetTagAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagDto>> GetTagAsync([FromRoute] int id)
    {
        var tag = await this.tagService.GetTagByIdAsync(id, this.HttpContext.RequestAborted);

        if (tag == null)
        {
            return this.NotFound($"Tag with ID {id} not found");
        }

        return this.Ok(tag);
    }
}