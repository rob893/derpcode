using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Controllers.V1;

[Route("api/v{version:apiVersion}/users/{userId}")]
[ApiVersion("1")]
[ApiController]
public sealed class UserFavoritesController : ServiceControllerBase
{
    private readonly IUserFavoriteService userFavoriteService;

    public UserFavoritesController(IUserFavoriteService userFavoriteService, ICorrelationIdService correlationIdService)
        : base(correlationIdService)
    {
        this.userFavoriteService = userFavoriteService ?? throw new ArgumentNullException(nameof(userFavoriteService));
    }

    /// <summary>
    /// Gets the user's favorite problems.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The user's favorite problems.</returns>
    /// <response code="200">Returns the user's favorite problems.</response>
    [HttpGet("favoriteProblems", Name = nameof(GetFavoriteProblemsForUserAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserFavoriteProblemDto>>> GetFavoriteProblemsForUserAsync([FromRoute] int userId)
    {
        var favoriteProblems = await this.userFavoriteService.GetFavoriteProblemsForUserAsync(userId, this.HttpContext.RequestAborted);

        if (!favoriteProblems.IsSuccess)
        {
            return this.HandleServiceFailureResult(favoriteProblems);
        }

        return this.Ok(favoriteProblems.ValueOrThrow);
    }

    /// <summary>
    /// Favorites a problem for the user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="problemId">The problem id.</param>
    /// <returns>The new favorite problem.</returns>
    /// <response code="200">Returns the new favorite problem.</response>
    [HttpPut("favoriteProblems/{problemId}", Name = nameof(FavoriteProblemForUserAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserFavoriteProblemDto>> FavoriteProblemForUserAsync([FromRoute] int userId, [FromRoute] int problemId)
    {
        var favoriteProblem = await this.userFavoriteService.FavoriteProblemForUserAsync(userId, problemId, this.HttpContext.RequestAborted);

        if (!favoriteProblem.IsSuccess)
        {
            return this.HandleServiceFailureResult(favoriteProblem);
        }

        return this.Ok(favoriteProblem.ValueOrThrow);
    }

    /// <summary>
    /// Unfavorites a problem for the user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="problemId">The problem id.</param>
    /// <response code="204">When the problem is unfavorited.</response>
    [HttpDelete("favoriteProblems/{problemId}", Name = nameof(UnfavoriteProblemForUserAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UnfavoriteProblemForUserAsync([FromRoute] int userId, [FromRoute] int problemId)
    {
        var res = await this.userFavoriteService.UnfavoriteProblemForUserAsync(userId, problemId, this.HttpContext.RequestAborted);

        if (!res.IsSuccess)
        {
            return this.HandleServiceFailureResult(res);
        }

        return this.NoContent();
    }
}