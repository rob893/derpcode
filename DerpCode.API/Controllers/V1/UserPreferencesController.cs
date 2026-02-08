using System;
using System.Threading.Tasks;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Controllers.V1;

[Route("api/v{version:apiVersion}/users/{userId}")]
[ApiVersion("1")]
[ApiController]
public sealed class UserPreferencesController : ServiceControllerBase
{
    private readonly IUserPreferencesServices userPreferencesServices;

    public UserPreferencesController(IUserPreferencesServices userPreferencesServices, ICorrelationIdService correlationIdService)
        : base(correlationIdService)
    {
        this.userPreferencesServices = userPreferencesServices ?? throw new ArgumentNullException(nameof(userPreferencesServices));
    }

    /// <summary>
    /// Gets the user's preferences.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The user's preferences.</returns>
    /// <response code="200">Returns the user's preferences.</response>
    /// <response code="404">When the user has no preferences.</response>
    [HttpGet("preferences", Name = nameof(GetUserPreferencesAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserPreferencesDto?>> GetUserPreferencesAsync([FromRoute] int userId)
    {
        var preferences = await this.userPreferencesServices.GetUserPreferencesAsync(userId, this.HttpContext.RequestAborted);

        if (!preferences.IsSuccess)
        {
            return this.HandleServiceFailureResult(preferences);
        }

        return this.Ok(preferences.ValueOrThrow);
    }

    /// <summary>
    /// Partially updates a user's preferences using JSON Patch.
    /// </summary>
    /// <param name="userId">The ID of the user who's preferecnes to update.</param>
    /// <param name="id">The ID of the preferecnes to update.</param>
    /// <param name="dtoPatchDoc">The JSON Patch document containing the updates.</param>
    /// <returns>The updated problem.</returns>
    /// <response code="200">Returns the updated preferences.</response>
    /// <response code="404">If the preferences are not found.</response>
    [HttpPatch("preferences/{id}", Name = nameof(UpdateUserPreferencesAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> UpdateUserPreferencesAsync([FromRoute] int userId, [FromRoute] int id, [FromBody] JsonPatchDocument<PatchUserPreferencesRequest> dtoPatchDoc)
    {
        var patchedResult = await this.userPreferencesServices.PatchPreferencesAsync(userId, id, dtoPatchDoc, this.HttpContext.RequestAborted);

        if (!patchedResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(patchedResult);
        }

        var patched = patchedResult.ValueOrThrow;

        return this.Ok(patched);
    }
}