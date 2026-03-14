using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public interface IUserProblemProgressRepository : IRepository<UserProblemProgress, string, CursorPaginationQueryParameters>
{
    Task<UserProblemProgress?> GetByUserAndProblemAsync(int userId, int problemId, bool track = true, CancellationToken cancellationToken = default);
}
