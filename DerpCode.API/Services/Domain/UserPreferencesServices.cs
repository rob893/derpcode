using System;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing user preference-related business logic
/// </summary>
public sealed class UserPreferencesServices : IUserPreferencesServices
{
    private readonly IUserPreferencesRepository userPreferencesRepository;

    private readonly ICurrentUserService currentUserService;

    private readonly ILogger<UserPreferencesServices> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPreferencesServices"/> class
    /// </summary>
    /// <param name="userPreferencesRepository">The repository</param>
    /// <param name="currentUserService">The current user service</param>
    /// <param name="logger">The logger</param>
    public UserPreferencesServices(IUserPreferencesRepository userPreferencesRepository, ICurrentUserService currentUserService, ILogger<UserPreferencesServices> logger)
    {
        this.userPreferencesRepository = userPreferencesRepository ?? throw new ArgumentNullException(nameof(userPreferencesRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<UserPreferencesDto>> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken)
    {
        if (this.currentUserService.UserId != userId && !this.currentUserService.IsAdmin)
        {
            this.logger.LogWarning("User {UserId} attempted to access user {TargetUserId} preferences without permission", this.currentUserService.UserId, userId);
            return Result<UserPreferencesDto>.Failure(DomainErrorType.Forbidden, "You can only see your own preferences.");
        }

        var preferences = await this.userPreferencesRepository.FirstOrDefaultAsync(x => x.UserId == userId, false, cancellationToken);

        if (preferences == null)
        {
            return Result<UserPreferencesDto>.Failure(DomainErrorType.NotFound, $"No preferences found for user {userId}.");
        }

        return Result<UserPreferencesDto>.Success(UserPreferencesDto.FromEntity(preferences));
    }

    /// <inheritdoc />
    public async Task<Result<UserPreferencesDto>> PatchPreferencesAsync(int userId, int id, JsonPatchDocument<PatchUserPreferencesRequest> patchDocument, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(patchDocument);

        if (this.currentUserService.UserId != userId && !this.currentUserService.IsAdmin)
        {
            this.logger.LogWarning("User {UserId} attempted to access user {TargetUserId} preferences without permission", this.currentUserService.UserId, userId);
            return Result<UserPreferencesDto>.Failure(DomainErrorType.Forbidden, "You can only see your own preferences.");
        }

        if (patchDocument.Operations.Count == 0)
        {
            this.logger.LogWarning("Cannot patch preferences {Id}: No operations provided in patch document", id);
            return Result<UserPreferencesDto>.Failure(DomainErrorType.Validation, "A JSON patch document with at least 1 operation is required.");
        }

        var preferences = await this.userPreferencesRepository.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, track: true, cancellationToken);

        if (preferences == null)
        {
            return Result<UserPreferencesDto>.Failure(DomainErrorType.NotFound, $"No preferences found for user {userId}.");
        }

        var validationCheck = PatchUserPreferencesRequest.FromEntity(preferences);
        if (!patchDocument.TryApply(validationCheck, out var validationError))
        {
            this.logger.LogWarning("Cannot patch preferences {Id}: Invalid patch document - {ValidationError}", id, validationError);
            return Result<UserPreferencesDto>.Failure(DomainErrorType.Validation, $"Invalid JSON patch document: {validationError}");
        }

        var entityPatchDoc = patchDocument.MapPatchDocument<PatchUserPreferencesRequest, UserPreferences>();

        if (!entityPatchDoc.TryApply(preferences, out var error))
        {
            this.logger.LogError("Failed to apply patch document to preferences {ProblemId} after successful validation: {Error}", id, error);
            return Result<UserPreferencesDto>.Failure(DomainErrorType.Unknown, $"Failed to apply JSON patch document after successful validation: {error}");
        }

        preferences.LastUpdated = DateTimeOffset.UtcNow;

        var updated = await this.userPreferencesRepository.SaveChangesAsync(cancellationToken);

        if (updated == 0)
        {
            this.logger.LogError("Failed to patch preferences {Id}: No changes were saved", id);
            return Result<UserPreferencesDto>.Failure(DomainErrorType.Unknown, "Failed to update preferences. No changes were saved.");
        }

        return Result<UserPreferencesDto>.Success(UserPreferencesDto.FromEntity(preferences));
    }
}
