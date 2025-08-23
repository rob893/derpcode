using System;
using System.Threading.Tasks;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
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
    public async Task<ActionResult<CursorPaginatedResponse<ArticleCommentDto>>> GetArticleCommentsAsync([FromRoute] int id, [FromQuery] ArticleCommentQueryParameters searchParams)
    {
        if (searchParams == null)
        {
            return this.BadRequest("Search parameters cannot be null.");
        }

        if (searchParams.ArticleId.HasValue)
        {
            return this.BadRequest("Article id is passed in the route, not the query.");
        }

        var paramsWithArticleId = searchParams with { ArticleId = id };
        var comments = await this.articleService.GetArticleCommentsAsync(paramsWithArticleId, this.HttpContext.RequestAborted);
        var response = comments.ToCursorPaginatedResponse(paramsWithArticleId);

        return this.Ok(response);
    }

    [HttpGet("{articleId}/comments/{commentId}", Name = nameof(GetArticleCommentAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleCommentDto?>> GetArticleCommentAsync([FromRoute] int articleId, [FromRoute] int commentId)
    {
        var comment = await this.articleService.GetArticleCommentByIdAsync(commentId, this.HttpContext.RequestAborted);

        if (comment == null || comment.ArticleId != articleId)
        {
            return this.NotFound($"Article comment with ID {commentId} not found.");
        }

        return this.Ok(comment);
    }

    [HttpGet("{articleId}/comments/{commentId}/replies", Name = nameof(GetArticleCommentRepliesAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ArticleCommentDto?>> GetArticleCommentRepliesAsync([FromRoute] int articleId, [FromRoute] int commentId, [FromQuery] ArticleCommentQueryParameters searchParams)
    {
        if (searchParams == null)
        {
            return this.BadRequest("Search parameters cannot be null.");
        }

        if (searchParams.ArticleId.HasValue)
        {
            return this.BadRequest("Article id is passed in the route, not the query.");
        }

        if (searchParams.ParentCommentId.HasValue)
        {
            return this.BadRequest("ParentCommentId id is passed in the route, not the query.");
        }

        if (searchParams.QuotedCommentId.HasValue)
        {
            return this.BadRequest("QuotedCommentId is not supported for this endpoint.");
        }

        var paramsWithArticleId = searchParams with
        {
            ArticleId = articleId,
            ParentCommentId = commentId
        };
        var comments = await this.articleService.GetArticleCommentsAsync(paramsWithArticleId, this.HttpContext.RequestAborted);
        var response = comments.ToCursorPaginatedResponse(paramsWithArticleId);

        return this.Ok(response);
    }

    [HttpGet("{articleId}/comments/{commentId}/quotedBy", Name = nameof(GetArticleCommentQuotedByAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ArticleCommentDto?>> GetArticleCommentQuotedByAsync([FromRoute] int articleId, [FromRoute] int commentId, [FromQuery] ArticleCommentQueryParameters searchParams)
    {
        if (searchParams == null)
        {
            return this.BadRequest("Search parameters cannot be null.");
        }

        if (searchParams.ArticleId.HasValue)
        {
            return this.BadRequest("Article id is passed in the route, not the query.");
        }

        if (searchParams.QuotedCommentId.HasValue)
        {
            return this.BadRequest("QuotedCommentId id is passed in the route, not the query.");
        }

        if (searchParams.ParentCommentId.HasValue)
        {
            return this.BadRequest("ParentCommentId is not supported for this endpoint.");
        }

        var paramsWithArticleId = searchParams with
        {
            ArticleId = articleId,
            QuotedCommentId = commentId
        };
        var comments = await this.articleService.GetArticleCommentsAsync(paramsWithArticleId, this.HttpContext.RequestAborted);
        var response = comments.ToCursorPaginatedResponse(paramsWithArticleId);

        return this.Ok(response);
    }

    [HttpPost("{articleId}/comments", Name = nameof(CreateArticleCommentAsync))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleCommentDto?>> CreateArticleCommentAsync([FromRoute] int articleId, [FromBody] CreateArticleCommentRequest createCommentRequest)
    {
        var newCommentResult = await this.articleService.CreateCommentAsync(articleId, createCommentRequest, this.HttpContext.RequestAborted);

        if (!newCommentResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(newCommentResult);
        }

        var newComment = newCommentResult.ValueOrThrow;

        return this.CreatedAtRoute(nameof(GetArticleCommentAsync), new { articleId, commentId = newComment.Id }, newComment);
    }
}