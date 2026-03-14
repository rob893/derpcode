using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public interface IUserProgressRepository : IRepository<UserProgress, CursorPaginationQueryParameters>
{
    Task<UserProgress?> GetByUserIdAsync(int userId, bool track = true, CancellationToken cancellationToken = default);
}
