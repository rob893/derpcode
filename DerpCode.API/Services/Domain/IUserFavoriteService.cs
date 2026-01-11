using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing user favorites business logic
/// </summary>
public interface IUserFavoriteService
{
    /// <summary>
    /// Gets the favorite problems for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of the user's favorite problems</returns>
    Task<Result<IReadOnlyList<UserFavoriteProblemDto>>> GetFavoriteProblemsForUserAsync(int userId, CancellationToken cancellationToken);

    /// <summary>
    /// Favorites a problem for a user.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="problemId">The ID of the problem</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<UserFavoriteProblemDto>> FavoriteProblemForUserAsync(int userId, int problemId, CancellationToken cancellationToken);

    /// <summary>
    /// Unfavorites a problem for a user.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="problemId">The ID of the problem</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> UnfavoriteProblemForUserAsync(int userId, int problemId, CancellationToken cancellationToken);
}
