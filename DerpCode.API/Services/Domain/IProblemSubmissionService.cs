using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Requests;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing problem submission-related business logic
/// </summary>
public interface IProblemSubmissionService
{
    /// <summary>
    /// Retrieves a single problem submission by ID
    /// </summary>
    /// <param name="problemId">The problem ID</param>
    /// <param name="submissionId">The submission ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The submission if found and user is authorized</returns>
    Task<Result<ProblemSubmissionDto>> GetProblemSubmissionAsync(int problemId, int submissionId, CancellationToken cancellationToken);

    /// <summary>
    /// Submits and saves a solution for a problem
    /// </summary>
    /// <param name="problemId">The problem ID</param>
    /// <param name="request">The submission request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created submission</returns>
    Task<Result<ProblemSubmissionDto>> SubmitSolutionAsync(int problemId, ProblemSubmissionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Runs a solution without saving it
    /// </summary>
    /// <param name="problemId">The problem ID</param>
    /// <param name="request">The submission request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The execution result</returns>
    Task<Result<ProblemSubmissionDto>> RunSolutionAsync(int problemId, ProblemSubmissionRequest request, CancellationToken cancellationToken);
}
