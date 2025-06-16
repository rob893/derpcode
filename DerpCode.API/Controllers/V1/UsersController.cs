using System;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Core;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Controllers.V1;

[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1")]
[ApiController]
public sealed class UsersController : ServiceControllerBase
{
    private readonly IUserService userService;

    private readonly ICurrentUserService currentUserService;

    public UsersController(
        IUserService userService,
        ICurrentUserService currentUserService,
        ICorrelationIdService correlationIdService)
        : base(correlationIdService)
    {
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Gets a paginated list of users.
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters for searching users.</param>
    /// <returns>A paginated response containing user DTOs.</returns>
    /// <response code="200">Returns the paginated list of users.</response>
    [HttpGet(Name = nameof(GetUsersAsync))]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<UserDto>>> GetUsersAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var users = await this.userService.GetUsersAsync(searchParams, this.HttpContext.RequestAborted);
        var response = users.ToCursorPaginatedResponse(searchParams);

        return this.Ok(response);
    }

    /// <summary>
    /// Gets a specific user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The user with the specified ID.</returns>
    /// <response code="200">Returns the user.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpGet("{id}", Name = nameof(GetUserAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserAsync([FromRoute] int id)
    {
        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return this.Forbidden("You can only see your own user information.");
        }

        var user = await this.userService.GetUserByIdAsync(id, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"User with id {id} does not exist.");
        }

        return this.Ok(user);
    }

    /// <summary>
    /// Deletes a user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">User was successfully deleted.</response>
    /// <response code="400">If the deletion failed.</response>
    /// <response code="401">If the user is not authorized to delete this user.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUserAsync([FromRoute] int id)
    {
        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return this.Forbidden("You can only delete your own user information.");
        }

        var deleteResult = await this.userService.DeleteUserAsync(id, this.HttpContext.RequestAborted);

        if (!deleteResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(deleteResult);
        }

        return this.NoContent();
    }

    /// <summary>
    /// Deletes a linked account from a user.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="linkedAccountType">The type of linked account to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Linked account was successfully deleted.</response>
    /// <response code="400">If the deletion failed.</response>
    /// <response code="401">If the user is not authorized to delete this linked account.</response>
    /// <response code="404">If the user or linked account is not found.</response>
    [HttpDelete("{id}/linkedAccounts/{linkedAccountType}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUserLinkedAccountAsync([FromRoute] int id, [FromRoute] LinkedAccountType linkedAccountType)
    {
        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return this.Forbidden("You can only delete your own linked accounts.");
        }

        var deleteResult = await this.userService.DeleteUserLinkedAccountAsync(id, linkedAccountType, this.HttpContext.RequestAborted);

        if (!deleteResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(deleteResult);
        }

        return this.NoContent();
    }

    /// <summary>
    /// Gets a paginated list of roles.
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters for searching roles.</param>
    /// <returns>A paginated response containing role DTOs.</returns>
    /// <response code="200">Returns the paginated list of roles.</response>
    [HttpGet("roles")]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<RoleDto>>> GetRolesAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var roles = await this.userService.GetRolesAsync(searchParams, this.HttpContext.RequestAborted);
        var response = roles.ToCursorPaginatedResponse(searchParams);

        return this.Ok(response);
    }

    /// <summary>
    /// Adds roles to a user. Admin access required.
    /// </summary>
    /// <param name="id">The ID of the user to add roles to.</param>
    /// <param name="roleEditDto">The role edit request containing the roles to add.</param>
    /// <returns>The updated user with the new roles.</returns>
    /// <response code="200">Returns the user with the added roles.</response>
    /// <response code="400">If the request is invalid or the role addition failed.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpPost("{id}/roles")]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> AddRolesAsync([FromRoute] int id, [FromBody] EditRoleRequest roleEditDto)
    {
        var addRolesResult = await this.userService.AddRolesToUserAsync(id, roleEditDto, this.HttpContext.RequestAborted);

        if (!addRolesResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(addRolesResult);
        }

        var user = addRolesResult.ValueOrThrow;

        return this.Ok(user);
    }

    /// <summary>
    /// Removes roles from a user. Admin access required.
    /// </summary>
    /// <param name="id">The ID of the user to remove roles from.</param>
    /// <param name="roleEditDto">The role edit request containing the roles to remove.</param>
    /// <returns>The updated user with the removed roles.</returns>
    /// <response code="200">Returns the user with the roles removed.</response>
    /// <response code="400">If the request is invalid or the role removal failed.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpDelete("{id}/roles")]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> RemoveRolesAsync([FromRoute] int id, [FromBody] EditRoleRequest roleEditDto)
    {
        var removeRolesResult = await this.userService.RemoveRolesFromUserAsync(id, roleEditDto, this.HttpContext.RequestAborted);

        if (!removeRolesResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(removeRolesResult);
        }

        var user = removeRolesResult.ValueOrThrow;

        return this.Ok(user);
    }

    /// <summary>
    /// Updates a user's username.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <param name="request">The update username request.</param>
    /// <returns>The updated user.</returns>
    /// <response code="200">If the user was updated.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="403">If the user is not authorized to update this password.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [HttpPut("{id}/username", Name = nameof(UpdateUsernameAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> UpdateUsernameAsync([FromRoute] int id, [FromBody] UpdateUsernameRequest request)
    {
        if (request == null)
        {
            return this.BadRequest();
        }

        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return this.Forbidden("You can only update your own user information.");
        }

        var updateResult = await this.userService.UpdateUsernameAsync(id, request, this.HttpContext.RequestAborted);

        if (!updateResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(updateResult);
        }

        var user = updateResult.ValueOrThrow;

        return this.Ok(user);
    }

    /// <summary>
    /// Updates a user's password.
    /// </summary>
    /// <param name="id">The ID of the user whose password is being updated.</param>
    /// <param name="request">The update password request.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the password was updated.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="403">If the user is not authorized to update this password.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [HttpPut("{id}/password", Name = nameof(UpdatePasswordAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UpdatePasswordAsync([FromRoute] int id, [FromBody] UpdatePasswordRequest request)
    {
        if (request == null)
        {
            return this.BadRequest();
        }

        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return this.Forbidden("You can only update your own password.");
        }

        var updateResult = await this.userService.UpdatePasswordAsync(id, request, this.HttpContext.RequestAborted);

        if (!updateResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(updateResult);
        }

        return this.NoContent();
    }

    /// <summary>
    /// Resends the email confirmation for the user's email.
    /// </summary>
    /// <param name="id">The ID of the user.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the email was sent.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="403">If the user is not authorized to resend the email.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [HttpPost("{id}/emailConfirmations", Name = nameof(SendEmailConfirmationAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> SendEmailConfirmationAsync([FromRoute] int id)
    {
        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return this.Forbidden("You can only resend confirmation email for your own account.");
        }

        var sendResult = await this.userService.SendEmailConfirmationAsync(id, this.HttpContext.RequestAborted);

        if (!sendResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(sendResult);
        }

        return this.NoContent();
    }

    /// <summary>
    /// Sends a link to reset password if a user forgot.
    /// </summary>
    /// <param name="request">The forgot password request.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the password reset link was sent.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [AllowAnonymous]
    [HttpPost("forgotPassword", Name = nameof(ForgotPasswordAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        // always return 204 to prevent user enumeration
        if (request == null)
        {
            return this.NoContent();
        }

        await this.userService.ForgotPasswordAsync(request, this.HttpContext.RequestAborted);

        return this.NoContent();
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="request">The reset password request.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the password was reset.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [AllowAnonymous]
    [HttpPost("resetPassword", Name = nameof(ResetPasswordAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        // always return 204 to prevent user enumeration
        if (request == null)
        {
            return this.NoContent();
        }

        await this.userService.ResetPasswordAsync(request, this.HttpContext.RequestAborted);

        return this.NoContent();
    }

    /// <summary>
    /// Confirms a user's email.
    /// </summary>
    /// <param name="request">The confirm email request.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the email was confirmed.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="500">If an unexpected server error occured.</response>
    /// <response code="504">If the server took too long to respond.</response>
    [AllowAnonymous]
    [HttpPost("emailConfirmations", Name = nameof(ConfirmEmailAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request)
    {
        if (request == null)
        {
            return this.BadRequest();
        }

        var confirmResult = await this.userService.ConfirmEmailAsync(request, this.HttpContext.RequestAborted);

        if (!confirmResult.IsSuccess)
        {
            return this.HandleServiceFailureResult(confirmResult);
        }

        return this.NoContent();
    }
}