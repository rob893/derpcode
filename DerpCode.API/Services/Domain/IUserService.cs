using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing user-related business logic
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves a paginated list of users
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated list of users</returns>
    Task<CursorPaginatedList<UserDto, int>> GetUsersAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a single user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<Result<UserDto>> GetUserByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> DeleteUserAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a linked account from a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="linkedAccountType">The type of linked account to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> DeleteUserLinkedAccountAsync(int userId, LinkedAccountType linkedAccountType, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated list of roles
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated list of roles</returns>
    Task<CursorPaginatedList<RoleDto, int>> GetRolesAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken);

    /// <summary>
    /// Adds roles to a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The role edit request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user</returns>
    Task<Result<UserDto>> AddRolesToUserAsync(int userId, EditRoleRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Removes roles from a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The role edit request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user</returns>
    Task<Result<UserDto>> RemoveRolesFromUserAsync(int userId, EditRoleRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a user's username
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The update username request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated user</returns>
    Task<Result<UserDto>> UpdateUsernameAsync(int userId, UpdateUsernameRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a user's password
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">The update password request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> UpdatePasswordAsync(int userId, UpdatePasswordRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an email confirmation to the user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> SendEmailConfirmationAsync(int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a password reset link to the user's email
    /// </summary>
    /// <param name="request">The forgot password request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure (always returns success to prevent user enumeration)</returns>
    Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Resets a user's password using a reset token
    /// </summary>
    /// <param name="request">The reset password request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure (always returns success to prevent user enumeration)</returns>
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Confirms a user's email using a confirmation token
    /// </summary>
    /// <param name="request">The confirm email request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken);
}
