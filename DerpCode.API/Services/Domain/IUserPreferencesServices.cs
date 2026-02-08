using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing user preferences business logic
/// </summary>
public interface IUserPreferencesServices
{
    /// <summary>
    /// Gets the user preferences for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    Task<Result<UserPreferencesDto>> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken);

    Task<Result<UserPreferencesDto>> PatchPreferencesAsync(int userId, int id, JsonPatchDocument<PatchUserPreferencesRequest> patchDocument, CancellationToken cancellationToken);
}
