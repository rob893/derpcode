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

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/articles")]
[ApiVersion("1.0")]
public sealed class ArticlesController : ServiceControllerBase
{
    private readonly IArticleService articleService;

    public ArticlesController(ICorrelationIdService correlationIdService, IArticleService articleService) : base(correlationIdService)
    {
        this.articleService = articleService ?? throw new ArgumentNullException(nameof(articleService));
    }

    [HttpGet(Name = nameof(GetArticlesAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<ArticleDto>>> GetArticlesAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var articles = await this.articleService.GetArticlesAsync(searchParams, this.HttpContext.RequestAborted);
        var response = articles.ToCursorPaginatedResponse(searchParams);

        return this.Ok(response);
    }

    [HttpGet("{id}", Name = nameof(GetArticleAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleDto?>> GetArticleAsync([FromRoute] int id)
    {
        var article = await this.articleService.GetArticleByIdAsync(id, this.HttpContext.RequestAborted);

        if (article == null)
        {
            return this.NotFound($"Article with ID {id} not found.");
        }

        return this.Ok(article);
    }

    [HttpGet("{id}/comments", Name = nameof(GetArticleCommentsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<ArticleCommentDto>>> GetArticleCommentsAsync([FromRoute] int id, [FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var templates = await this.articleService.GetArticlesAsync(searchParams, this.HttpContext.RequestAborted);
        var response = templates.ToCursorPaginatedResponse(searchParams);

        return this.Ok(response);
    }
}