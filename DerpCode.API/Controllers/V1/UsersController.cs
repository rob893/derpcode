using System;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Models.Settings;
using DerpCode.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DerpCode.API.Controllers.V1;

[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1")]
[ApiController]
public sealed class UsersController : ServiceControllerBase
{
    private readonly IUserRepository userRepository;

    private readonly AuthenticationSettings authSettings;

    private readonly IEmailService emailService;

    private readonly IEmailTemplateService emailTemplateService;

    private readonly ILogger<UsersController> logger;

    public UsersController(
        IUserRepository userRepository,
        IOptions<AuthenticationSettings> authSettings,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        ILogger<UsersController> logger,
        ICorrelationIdService correlationIdService)
        : base(correlationIdService)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.authSettings = authSettings?.Value ?? throw new ArgumentNullException(nameof(authSettings));
        this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        this.emailTemplateService = emailTemplateService ?? throw new ArgumentNullException(nameof(emailTemplateService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a paginated list of users.
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters for searching users.</param>
    /// <returns>A paginated response containing user DTOs.</returns>
    /// <response code="200">Returns the paginated list of users.</response>
    [HttpGet(Name = nameof(GetUsersAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<UserDto>>> GetUsersAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var users = await this.userRepository.SearchAsync(searchParams, track: false, this.HttpContext.RequestAborted);
        var paginatedResponse = users.Select(UserDto.FromEntity).ToCursorPaginatedResponse(searchParams);

        return this.Ok(paginatedResponse);
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
        var user = await this.userRepository.GetByIdAsync(id, track: false, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"User with id {id} does not exist.");
        }

        var userToReturn = UserDto.FromEntity(user);

        return this.Ok(userToReturn);
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
        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No User with Id {id} found.");
        }

        if (!this.IsUserAuthorizedForResource(user.Id))
        {
            return this.Unauthorized("You can only delete your own user.");
        }

        this.userRepository.Remove(user);
        var saveResults = await this.userRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        if (saveResults == 0)
        {
            return this.BadRequest("Failed to delete the user.");
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
        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No User with Id {id} found.");
        }

        if (!this.IsUserAuthorizedForResource(user.Id))
        {
            return this.Unauthorized("You can only delete your own linked accounts.");
        }

        var linkedAccount = user.LinkedAccounts.FirstOrDefault(account => account.LinkedAccountType == linkedAccountType);

        if (linkedAccount == null)
        {
            return this.NotFound($"No linked account of type {linkedAccountType} found for user with Id {id}.");
        }

        user.LinkedAccounts.Remove(linkedAccount);
        var saveResults = await this.userRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        if (saveResults == 0)
        {
            return this.BadRequest("Failed to delete the linked account.");
        }

        return this.NoContent();
    }

    /// <summary>
    /// Updates a user using JSON Patch operations.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="dtoPatchDoc">The JSON patch document containing the operations to apply.</param>
    /// <returns>The updated user.</returns>
    /// <response code="200">Returns the updated user.</response>
    /// <response code="400">If the patch document is invalid or the update failed.</response>
    /// <response code="401">If the user is not authorized to update this user.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUserAsync([FromRoute] int id, [FromBody] JsonPatchDocument<UpdateUserRequest> dtoPatchDoc)
    {
        if (dtoPatchDoc == null || dtoPatchDoc.Operations.Count == 0)
        {
            return this.BadRequest("A JSON patch document with at least 1 operation is required.");
        }

        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No user with Id {id} found.");
        }

        if (!this.User.TryGetUserId(out var userId))
        {
            return this.Unauthorized("You cannot do this.");
        }

        if (!this.User.IsAdmin() && userId != user.Id)
        {
            return this.Unauthorized("You cannot do this.");
        }

        var patchDoc = dtoPatchDoc.MapPatchDocument<UpdateUserRequest, User>();

        if (!patchDoc.TryApply(user, out var error))
        {
            return this.BadRequest($"Invalid JSON patch document: {error}");
        }

        await this.userRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        var userToReturn = UserDto.FromEntity(user);

        return this.Ok(userToReturn);
    }

    /// <summary>
    /// Gets a paginated list of roles.
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters for searching roles.</param>
    /// <returns>A paginated response containing role DTOs.</returns>
    /// <response code="200">Returns the paginated list of roles.</response>
    [HttpGet("roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<RoleDto>>> GetRolesAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var roles = await this.userRepository.GetRolesAsync(searchParams, this.HttpContext.RequestAborted);
        var paginatedResponse = roles.Select(RoleDto.FromEntity).ToCursorPaginatedResponse(searchParams);

        return this.Ok(paginatedResponse);
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
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [HttpPost("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> AddRolesAsync([FromRoute] int id, [FromBody] EditRoleRequest roleEditDto)
    {
        if (roleEditDto == null || roleEditDto.RoleNames == null || roleEditDto.RoleNames.Count == 0)
        {
            return this.BadRequest("At least one role must be specified.");
        }

        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No user with id {id} exists.");
        }

        var roles = await this.userRepository.GetRolesAsync(this.HttpContext.RequestAborted);
        var userRoles = user.UserRoles.Select(ur => ur.Role.Name?.ToUpperInvariant()).ToHashSet();
        var selectedRoles = roleEditDto.RoleNames.Select(role => role.ToUpperInvariant()).ToHashSet();

        var rolesToAdd = roles.Where(role =>
        {
            var upperName = role.Name?.ToUpperInvariant() ?? string.Empty;
            return selectedRoles.Contains(upperName) && !userRoles.Contains(upperName);
        });

        if (!rolesToAdd.Any())
        {
            return this.Ok(UserDto.FromEntity(user));
        }

        user.UserRoles.AddRange(rolesToAdd.Select(role => new UserRole
        {
            Role = role
        }));

        var saveResult = await this.userRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        if (saveResult == 0)
        {
            return this.BadRequest("Failed to add roles.");
        }

        var userToReturn = UserDto.FromEntity(user);

        return this.Ok(userToReturn);
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
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [HttpDelete("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> RemoveRolesAsync([FromRoute] int id, [FromBody] EditRoleRequest roleEditDto)
    {
        if (roleEditDto == null || roleEditDto.RoleNames == null || roleEditDto.RoleNames.Count == 0)
        {
            return this.BadRequest("At least one role must be specified.");
        }

        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No user with id {id} exists.");
        }

        var roles = await this.userRepository.GetRolesAsync(this.HttpContext.RequestAborted);
        var userRoles = user.UserRoles.Select(ur => ur.Role.Name?.ToUpperInvariant()).ToHashSet();
        var selectedRoles = roleEditDto.RoleNames.Select(role => role.ToUpperInvariant()).ToHashSet();

        var roleIdsToRemove = roles.Where(role =>
        {
            var upperName = role.Name?.ToUpperInvariant() ?? string.Empty;
            return selectedRoles.Contains(upperName) && userRoles.Contains(upperName);
        }).Select(role => role.Id).ToHashSet();

        if (roleIdsToRemove.Count == 0)
        {
            return this.Ok(UserDto.FromEntity(user));
        }

        user.UserRoles.RemoveAll(ur => roleIdsToRemove.Contains(ur.RoleId));
        var saveResult = await this.userRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        if (saveResult == 0)
        {
            return this.BadRequest("Failed to remove roles.");
        }

        var userToReturn = UserDto.FromEntity(user);

        return this.Ok(userToReturn);
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

        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null || string.IsNullOrWhiteSpace(user.Email) || !user.EmailConfirmed)
        {
            this.logger.LogWarning(
                "Failed to send password reset link for user with email {Email}: User not found or email not confirmed.",
                request.Email);
            return this.NoContent();
        }

        var token = await this.userRepository.UserManager.GeneratePasswordResetTokenAsync(user);

        var confLink = $"{this.authSettings.UIBaseUrl}#/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
        var (plainTextMessage, htmlMessage) = await this.emailTemplateService.GetPasswordResetTemplateAsync(confLink, this.HttpContext.RequestAborted);
        await this.emailService.SendEmailToUserAsync(user, "DerpCode Password Reset - Let's Get You Back In! 🔑", plainTextMessage, htmlMessage, this.HttpContext.RequestAborted);

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

        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null || !user.EmailConfirmed)
        {
            this.logger.LogWarning(
                "Failed to reset password for user with email {Email}: User not found or email not confirmed.",
                request.Email);
            return this.NoContent();
        }

        var result = await this.userRepository.UserManager.ResetPasswordAsync(user, request.Token, request.Password);

        if (!result.Succeeded)
        {
            this.logger.LogWarning(
                "Failed to reset password for user {UserId} ({Email}): {Errors}",
                user.Id,
                user.Email,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

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
    [HttpPost("confirmEmail", Name = nameof(ConfirmEmailAsync))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request)
    {
        if (request == null)
        {
            return this.BadRequest();
        }

        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return this.BadRequest("Unable to confirm email.");
        }

        var confirmResult = await this.userRepository.UserManager.ConfirmEmailAsync(user, request.Token);

        if (!confirmResult.Succeeded)
        {
            return this.BadRequest([.. confirmResult.Errors.Select(e => e.Description)]);
        }

        return this.NoContent();
    }
}