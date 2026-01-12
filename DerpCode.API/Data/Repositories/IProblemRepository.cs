
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public interface IProblemRepository : IRepository<Problem, CursorPaginationQueryParameters>
{
    /// <summary>
    /// Retrieves a personalized list of problems with limited data for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of personalized problems with limited details</returns>
    Task<IReadOnlyList<PersonalizedProblemLimitedDto>> GetPersonalizedProblemListAsync(int userId, CancellationToken cancellationToken);
}