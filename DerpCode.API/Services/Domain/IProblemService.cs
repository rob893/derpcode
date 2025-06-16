using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Requests;
using DerpCode.API.Models.Responses;
using Microsoft.AspNetCore.JsonPatch;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing problem-related business logic
/// </summary>
public interface IProblemService
{
    /// <summary>
    /// Retrieves a paginated list of problems
    /// </summary>
    /// <param name="searchParams">The cursor pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated list of problems</returns>
    Task<CursorPaginatedList<ProblemDto, int>> GetProblemsAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a single problem by ID
    /// </summary>
    /// <param name="id">The problem ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The problem if found, null otherwise</returns>
    Task<ProblemDto?> GetProblemByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new problem
    /// </summary>
    /// <param name="request">The create problem request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created problem</returns>
    Task<Result<ProblemDto>> CreateProblemAsync(CreateProblemRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Clones an existing problem
    /// </summary>
    /// <param name="existingProblemId">The id of the existing problem to clone</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cloned problem</returns>
    Task<Result<ProblemDto>> CloneProblemAsync(int existingProblemId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a problem using a JSON patch document (validation should be done first)
    /// </summary>
    /// <param name="problemId">The ID of the problem to update</param>
    /// <param name="patchDocument">The JSON patch document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated problem</returns>
    Task<Result<ProblemDto>> PatchProblemAsync(int problemId, JsonPatchDocument<CreateProblemRequest> patchDocument, CancellationToken cancellationToken);

    /// <summary>
    /// Performs a full update of a problem (problem existence should be validated first)
    /// </summary>
    /// <param name="problemId">The ID of the problem to update</param>
    /// <param name="request">The update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated problem</returns>
    Task<Result<ProblemDto>> UpdateProblemAsync(int problemId, CreateProblemRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a problem (problem existence should be validated first)
    /// </summary>
    /// <param name="problemId">The ID of the problem to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task<Result<bool>> DeleteProblemAsync(int problemId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates a problem creation request by testing all driver templates
    /// </summary>
    /// <param name="request">The create problem request to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The validation response</returns>
    Task<CreateProblemValidationResponse> ValidateCreateProblemAsync(CreateProblemRequest request, CancellationToken cancellationToken);
}
