using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Services.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing user favorites-related business logic
/// </summary>
public sealed class UserFavoriteService : IUserFavoriteService
{
    private readonly ILogger<UserFavoriteService> logger;

    private readonly IUserRepository userRepository;

    private readonly ICurrentUserService currentUserService;

    private readonly IMemoryCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserFavoriteService"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="userRepository">The user repository</param>
    /// <param name="currentUserService">The current user service</param>
    /// <param name="cache">The memory cache</param>
    public UserFavoriteService(
        ILogger<UserFavoriteService> logger,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IMemoryCache cache)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<UserFavoriteProblemDto>>> GetFavoriteProblemsForUserAsync(int userId, CancellationToken cancellationToken)
    {
        if (this.currentUserService.UserId != userId && !this.currentUserService.IsAdmin)
        {
            this.logger.LogWarning("User {UserId} attempted to access user {TargetUserId} without permission", this.currentUserService.UserId, userId);
            return Result<IReadOnlyList<UserFavoriteProblemDto>>.Failure(DomainErrorType.Forbidden, "You can only favorite problems for yourself.");
        }

        var favorites = await this.userRepository.GetFavoriteProblemsForUserAsync(userId, cancellationToken);

        return Result<IReadOnlyList<UserFavoriteProblemDto>>.Success([.. favorites.Select(UserFavoriteProblemDto.FromEntity)]);
    }

    /// <inheritdoc />
    public async Task<Result<UserFavoriteProblemDto>> FavoriteProblemForUserAsync(int userId, int problemId, CancellationToken cancellationToken)
    {
        if (this.currentUserService.UserId != userId && !this.currentUserService.IsAdmin)
        {
            this.logger.LogWarning("User {UserId} attempted to access user {TargetUserId} without permission", this.currentUserService.UserId, userId);
            return Result<UserFavoriteProblemDto>.Failure(DomainErrorType.Forbidden, "You can only favorite problems for yourself.");
        }

        try
        {
            var favorite = await this.userRepository.FavoriteProblemForUserAsync(userId, problemId, cancellationToken);

            this.cache.Remove(CacheKeys.GetPersonalizedProblemsKey(userId));

            return Result<UserFavoriteProblemDto>.Success(UserFavoriteProblemDto.FromEntity(favorite));
        }
        catch (KeyNotFoundException)
        {
            this.logger.LogWarning("User {UserId} attempted to favorite a problem {ProblemId} that does not exist or for a user {TargetUserId} that does not exist", this.currentUserService.UserId, problemId, userId);
            return Result<UserFavoriteProblemDto>.Failure(DomainErrorType.NotFound, "User or problem not found.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> UnfavoriteProblemForUserAsync(int userId, int problemId, CancellationToken cancellationToken)
    {
        if (this.currentUserService.UserId != userId && !this.currentUserService.IsAdmin)
        {
            this.logger.LogWarning("User {UserId} attempted to access user {TargetUserId} without permission", this.currentUserService.UserId, userId);
            return Result<bool>.Failure(DomainErrorType.Forbidden, "You can only unfavorite your own problems.");
        }

        var res = await this.userRepository.UnfavoriteProblemForUserAsync(userId, problemId, cancellationToken);

        if (res)
        {
            this.cache.Remove(CacheKeys.GetPersonalizedProblemsKey(userId));
        }

        return Result<bool>.Success(res);
    }
}