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
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DerpCode.API.Controllers.V1;

[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1")]
[ApiController]
public sealed class UsersController : ServiceControllerBase
{
    private readonly IUserRepository userRepository;

    public UsersController(IUserRepository userRepository, ICorrelationIdService correlationIdService)
        : base(correlationIdService)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpGet]
    public async Task<ActionResult<CursorPaginatedResponse<UserDto>>> GetUsersAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var users = await this.userRepository.SearchAsync(searchParams, track: false, this.HttpContext.RequestAborted);
        var paginatedResponse = users.Select(UserDto.FromEntity).ToCursorPaginatedResponse(searchParams);

        return this.Ok(paginatedResponse);
    }

    [HttpGet("{id}", Name = nameof(GetUserAsync))]
    public async Task<ActionResult<UserDto>> GetUserAsync(int id)
    {
        var user = await this.userRepository.GetByIdAsync(id, track: false, this.HttpContext.RequestAborted);

        if (user == null)
        {
            return this.NotFound($"User with id {id} does not exist.");
        }

        var userToReturn = UserDto.FromEntity(user);

        return this.Ok(userToReturn);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUserAsync(int id)
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

    [HttpPatch("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUserAsync(int id, [FromBody] JsonPatchDocument<UpdateUserRequest> dtoPatchDoc)
    {
        if (dtoPatchDoc == null || dtoPatchDoc.Operations.Count == 0)
        {
            return this.BadRequest("A JSON patch document with at least 1 operation is required.");
        }

        if (!dtoPatchDoc.IsValid(out var errors))
        {
            return this.BadRequest(errors);
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

        patchDoc.ApplyTo(user);

        await this.userRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        var userToReturn = UserDto.FromEntity(user);

        return this.Ok(userToReturn);
    }

    [HttpGet("roles")]
    public async Task<ActionResult<CursorPaginatedResponse<RoleDto>>> GetRolesAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var roles = await this.userRepository.GetRolesAsync(searchParams, this.HttpContext.RequestAborted);
        var paginatedResponse = roles.Select(RoleDto.FromEntity).ToCursorPaginatedResponse(searchParams);

        return this.Ok(paginatedResponse);
    }

    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [HttpPost("{id}/roles")]
    public async Task<ActionResult<UserDto>> AddRolesAsync(int id, [FromBody] EditRoleRequest roleEditDto)
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

    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [HttpDelete("{id}/roles")]
    public async Task<ActionResult<UserDto>> RemoveRolesAsync(int id, [FromBody] EditRoleRequest roleEditDto)
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
}