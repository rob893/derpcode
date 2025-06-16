using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Email;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing user-related business logic
/// </summary>
public sealed class UserService : IUserService
{
    private readonly ILogger<UserService> logger;

    private readonly IUserRepository userRepository;

    private readonly IEmailService emailService;

    private readonly ICurrentUserService currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="userRepository">The user repository</param>
    /// <param name="emailService">The email service</param>
    /// <param name="currentUserService">The current user service</param>
    public UserService(
        ILogger<UserService> logger,
        IUserRepository userRepository,
        IEmailService emailService,
        ICurrentUserService currentUserService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <inheritdoc />
    public async Task<CursorPaginatedList<UserDto, int>> GetUsersAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(searchParams);

        var pagedList = await this.userRepository.SearchAsync(searchParams, track: false, cancellationToken);
        var mapped = pagedList
            .Select(UserDto.FromEntity)
            .ToList();

        return new CursorPaginatedList<UserDto, int>(mapped, pagedList.HasNextPage, pagedList.HasPreviousPage, pagedList.StartCursor, pagedList.EndCursor, pagedList.TotalCount);
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> GetUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return Result<UserDto>.Failure(DomainErrorType.Forbidden, "You can only see your own user");
        }

        var user = await this.userRepository.GetByIdAsync(id, track: false, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure(DomainErrorType.NotFound, "User not found");
        }

        return Result<UserDto>.Success(UserDto.FromEntity(user));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        if (this.currentUserService.UserId != id && !this.currentUserService.IsAdmin)
        {
            return Result<bool>.Failure(DomainErrorType.Forbidden, "You can only delete your own user");
        }

        var user = await this.userRepository.GetByIdAsync(id, track: true, cancellationToken);

        if (user == null)
        {
            return Result<bool>.Failure(DomainErrorType.NotFound, "User not found");
        }

        this.userRepository.Remove(user);
        var saveResults = await this.userRepository.SaveChangesAsync(cancellationToken);

        if (saveResults == 0)
        {
            return Result<bool>.Failure(DomainErrorType.Unknown, "Failed to delete the user");
        }

        return Result<bool>.Success(true);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> DeleteUserLinkedAccountAsync(int userId, LinkedAccountType linkedAccountType, CancellationToken cancellationToken)
    {
        if (this.currentUserService.UserId != userId && !this.currentUserService.IsAdmin)
        {
            return Result<bool>.Failure(DomainErrorType.Forbidden, "You can only delete linked accounts for your own user");
        }

        var user = await this.userRepository.GetByIdAsync(userId, track: true, cancellationToken);

        if (user == null)
        {
            return Result<bool>.Failure(DomainErrorType.NotFound, "User not found");
        }

        var linkedAccount = user.LinkedAccounts.FirstOrDefault(account => account.LinkedAccountType == linkedAccountType);

        if (linkedAccount == null)
        {
            return Result<bool>.Failure(DomainErrorType.NotFound, $"No linked account of type {linkedAccountType} found for user");
        }

        user.LinkedAccounts.Remove(linkedAccount);
        var saveResults = await this.userRepository.SaveChangesAsync(cancellationToken);

        if (saveResults == 0)
        {
            return Result<bool>.Failure(DomainErrorType.Unknown, "Failed to delete the linked account");
        }

        return Result<bool>.Success(true);
    }

    /// <inheritdoc />
    public async Task<CursorPaginatedList<RoleDto, int>> GetRolesAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(searchParams);

        var pagedList = await this.userRepository.GetRolesAsync(searchParams, cancellationToken);
        var mapped = pagedList
            .Select(RoleDto.FromEntity)
            .ToList();

        return new CursorPaginatedList<RoleDto, int>(mapped, pagedList.HasNextPage, pagedList.HasPreviousPage, pagedList.StartCursor, pagedList.EndCursor, pagedList.TotalCount);
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> AddRolesToUserAsync(int userId, EditRoleRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RoleNames == null || request.RoleNames.Count == 0)
        {
            return Result<UserDto>.Failure(DomainErrorType.Validation, "At least one role must be specified");
        }

        var user = await this.userRepository.GetByIdAsync(userId, track: true, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure(DomainErrorType.NotFound, "User not found");
        }

        var roles = await this.userRepository.GetRolesAsync(cancellationToken);
        var userRoles = user.UserRoles.Select(ur => ur.Role.Name?.ToUpperInvariant()).ToHashSet();
        var selectedRoles = request.RoleNames.Select(role => role.ToUpperInvariant()).ToHashSet();

        var rolesToAdd = roles.Where(role =>
        {
            var upperName = role.Name?.ToUpperInvariant() ?? string.Empty;
            return selectedRoles.Contains(upperName) && !userRoles.Contains(upperName);
        });

        if (!rolesToAdd.Any())
        {
            return Result<UserDto>.Success(UserDto.FromEntity(user));
        }

        user.UserRoles.AddRange(rolesToAdd.Select(role => new UserRole
        {
            Role = role
        }));

        var saveResult = await this.userRepository.SaveChangesAsync(cancellationToken);

        if (saveResult == 0)
        {
            return Result<UserDto>.Failure(DomainErrorType.Unknown, "Failed to add roles");
        }

        return Result<UserDto>.Success(UserDto.FromEntity(user));
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> RemoveRolesFromUserAsync(int userId, EditRoleRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RoleNames == null || request.RoleNames.Count == 0)
        {
            return Result<UserDto>.Failure(DomainErrorType.Validation, "At least one role must be specified");
        }

        var user = await this.userRepository.GetByIdAsync(userId, track: true, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure(DomainErrorType.NotFound, "User not found");
        }

        var roles = await this.userRepository.GetRolesAsync(cancellationToken);
        var userRoles = user.UserRoles.Select(ur => ur.Role.Name?.ToUpperInvariant()).ToHashSet();
        var selectedRoles = request.RoleNames.Select(role => role.ToUpperInvariant()).ToHashSet();

        var roleIdsToRemove = roles.Where(role =>
        {
            var upperName = role.Name?.ToUpperInvariant() ?? string.Empty;
            return selectedRoles.Contains(upperName) && userRoles.Contains(upperName);
        }).Select(role => role.Id).ToHashSet();

        if (roleIdsToRemove.Count == 0)
        {
            return Result<UserDto>.Success(UserDto.FromEntity(user));
        }

        user.UserRoles.RemoveAll(ur => roleIdsToRemove.Contains(ur.RoleId));
        var saveResult = await this.userRepository.SaveChangesAsync(cancellationToken);

        if (saveResult == 0)
        {
            return Result<UserDto>.Failure(DomainErrorType.Unknown, "Failed to remove roles");
        }

        return Result<UserDto>.Success(UserDto.FromEntity(user));
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> UpdateUsernameAsync(int userId, UpdateUsernameRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (this.currentUserService.UserId != userId && !this.currentUserService.IsAdmin)
        {
            return Result<UserDto>.Failure(DomainErrorType.Forbidden, "You can only update your own username");
        }

        var user = await this.userRepository.GetByIdAsync(userId, track: true, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure(DomainErrorType.NotFound, "User not found");
        }

        if (user.LastLogin is null || user.LastLogin <= DateTimeOffset.UtcNow.AddMinutes(-30))
        {
            return Result<UserDto>.Failure(DomainErrorType.Validation, "You must have authenticated within the last 30 minutes to update your username");
        }

        if (user.LastUsernameChange > DateTimeOffset.UtcNow.AddDays(-30))
        {
            return Result<UserDto>.Failure(DomainErrorType.Validation, "You can only change your username once every 30 days");
        }

        if (string.Equals(user.UserName, request.NewUsername, StringComparison.OrdinalIgnoreCase))
        {
            return Result<UserDto>.Failure(DomainErrorType.Validation, "The new username must be different from the current one");
        }

        user.LastUsernameChange = DateTimeOffset.UtcNow;
        var updateResult = await this.userRepository.UserManager.SetUserNameAsync(user, request.NewUsername);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure(DomainErrorType.Validation, errors);
        }

        return Result<UserDto>.Success(UserDto.FromEntity(user));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> UpdatePasswordAsync(int userId, UpdatePasswordRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (this.currentUserService.UserId != userId && !this.currentUserService.IsAdmin)
        {
            return Result<bool>.Failure(DomainErrorType.Forbidden, "You can only update your own password");
        }

        var user = await this.userRepository.GetByIdAsync(userId, track: true, cancellationToken);

        if (user == null)
        {
            return Result<bool>.Failure(DomainErrorType.NotFound, "User not found");
        }

        user.LastPasswordChange = DateTimeOffset.UtcNow;
        var result = await this.userRepository.UserManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<bool>.Failure(DomainErrorType.Validation, errors);
        }

        return Result<bool>.Success(true);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> SendEmailConfirmationAsync(int userId, CancellationToken cancellationToken)
    {
        if (this.currentUserService.UserId != userId && !this.currentUserService.IsAdmin)
        {
            return Result<bool>.Failure(DomainErrorType.Forbidden, "You can only send email confirmation links for your own account");
        }

        var user = await this.userRepository.GetByIdAsync(userId, track: true, cancellationToken);

        if (user == null)
        {
            return Result<bool>.Failure(DomainErrorType.NotFound, "User not found");
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return Result<bool>.Failure(DomainErrorType.Validation, "User does not have an email address set");
        }

        if (user.EmailConfirmed)
        {
            return Result<bool>.Failure(DomainErrorType.Validation, "User's email is already confirmed");
        }

        if (user.LastEmailConfirmationSent != null && user.LastEmailConfirmationSent.Value > DateTimeOffset.UtcNow.AddHours(-1))
        {
            return Result<bool>.Failure(DomainErrorType.Validation, "You can only resend the email confirmation link once per hour");
        }

        var token = await this.userRepository.UserManager.GenerateEmailConfirmationTokenAsync(user);

        await this.emailService.SendEmailConfirmationToUserAsync(user, token, cancellationToken);

        user.LastEmailConfirmationSent = DateTimeOffset.UtcNow;
        await this.userRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Always return success to prevent user enumeration - log internally for failures
        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null || string.IsNullOrWhiteSpace(user.Email) || !user.EmailConfirmed)
        {
            this.logger.LogWarning(
                "Failed to send password reset link for user with email {Email}: User not found or email not confirmed.",
                request.Email);
            return Result<bool>.Success(true);
        }

        if (user.LastPasswordChange > DateTimeOffset.UtcNow.AddHours(-1))
        {
            this.logger.LogWarning(
                "Failed to send password reset link for user {UserId}: User has changed their password within the last hour.",
                user.Id);
            return Result<bool>.Success(true);
        }

        var token = await this.userRepository.UserManager.GeneratePasswordResetTokenAsync(user);

        await this.emailService.SendResetPasswordToUserAsync(user, token, cancellationToken);

        return Result<bool>.Success(true);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Always return success to prevent user enumeration - log internally for failures
        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null || !user.EmailConfirmed)
        {
            this.logger.LogWarning(
                "Failed to reset password for user with email {Email}: User not found or email not confirmed.",
                request.Email);
            return Result<bool>.Success(true);
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

        return Result<bool>.Success(true);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Result<bool>.Failure(DomainErrorType.Validation, "Unable to confirm email");
        }

        var confirmResult = await this.userRepository.UserManager.ConfirmEmailAsync(user, request.Token);

        if (!confirmResult.Succeeded)
        {
            var errors = string.Join(", ", confirmResult.Errors.Select(e => e.Description));
            return Result<bool>.Failure(DomainErrorType.Validation, errors);
        }

        return Result<bool>.Success(true);
    }
}
