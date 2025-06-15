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
using DerpCode.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V1;

[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1")]
[ApiController]
public sealed class UsersController : ServiceControllerBase
{
    private readonly IUserRepository userRepository;

    private readonly IEmailService emailService;

    private readonly ICurrentUserService currentUserService;

    private readonly ILogger<UsersController> logger;

    public UsersController(
        IUserRepository userRepository,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        ILogger<UsersController> logger,
        ICorrelationIdService correlationIdService)
        : base(correlationIdService)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return this.Forbidden("You can only see your own user information.");
        }

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
        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return this.Forbidden("You can only delete your own user information.");
        }

        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No User with Id {id} found.");
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
        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return this.Forbidden("You can only delete your own linked accounts.");
        }

        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No User with Id {id} found.");
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
    [HttpPost("{id}/roles")]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
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
    [HttpDelete("{id}/roles")]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
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

        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No user with Id {id} found.");
        }

        if (user.LastLogin is null || user.LastLogin <= DateTimeOffset.UtcNow.AddMinutes(-30))
        {
            return this.BadRequest("You must have authenticated within the last 30 minutes to update your username.");
        }

        if (user.LastUsernameChange > DateTimeOffset.UtcNow.AddDays(-30))
        {
            return this.BadRequest("You can only change your username once every 30 days.");
        }

        if (string.Equals(user.UserName, request.NewUsername, StringComparison.OrdinalIgnoreCase))
        {
            return this.BadRequest("The new username must be different from the current one.");
        }

        user.LastUsernameChange = DateTimeOffset.UtcNow;
        var updateResult = await this.userRepository.UserManager.SetUserNameAsync(user, request.NewUsername);

        if (!updateResult.Succeeded)
        {
            return this.BadRequest([.. updateResult.Errors.Select(e => e.Description)]);
        }

        var userToReturn = UserDto.FromEntity(user);

        return this.Ok(userToReturn);
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

        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No user with Id {id} found.");
        }

        user.LastPasswordChange = DateTimeOffset.UtcNow;
        var result = await this.userRepository.UserManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            return this.BadRequest([.. result.Errors.Select(e => e.Description)]);
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

        var user = await this.userRepository.GetByIdAsync(id, track: true, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"No user with Id {id} found.");
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return this.BadRequest("User does not have an email address set.");
        }

        if (user.EmailConfirmed)
        {
            return this.BadRequest("User's email is already confirmed.");
        }

        if (user.LastEmailConfirmationSent != null && user.LastEmailConfirmationSent.Value > DateTimeOffset.UtcNow.AddHours(-1))
        {
            return this.BadRequest("You can only resend the email confirmation link once per hour.");
        }

        var token = await this.userRepository.UserManager.GenerateEmailConfirmationTokenAsync(user);

        await this.emailService.SendEmailConfirmationToUserAsync(user, token, this.HttpContext.RequestAborted);

        user.LastEmailConfirmationSent = DateTimeOffset.UtcNow;
        await this.userRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

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

        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null || string.IsNullOrWhiteSpace(user.Email) || !user.EmailConfirmed)
        {
            this.logger.LogWarning(
                "Failed to send password reset link for user with email {Email}: User not found or email not confirmed.",
                request.Email);
            return this.NoContent();
        }

        if (user.LastPasswordChange > DateTimeOffset.UtcNow.AddHours(-1))
        {
            this.logger.LogWarning(
                "Failed to send password reset link for user {UserId}: User has changed their password within the last hour.",
                user.Id);
            return this.NoContent();
        }

        var token = await this.userRepository.UserManager.GeneratePasswordResetTokenAsync(user);

        await this.emailService.SendResetPasswordToUserAsync(user, token, this.HttpContext.RequestAborted);

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
    [HttpPost("emailConfirmations", Name = nameof(ConfirmEmailAsync))]
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