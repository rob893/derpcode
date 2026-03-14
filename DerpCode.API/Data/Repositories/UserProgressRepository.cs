using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Data.Repositories;

public sealed class UserProgressRepository(DataContext context) : Repository<UserProgress, CursorPaginationQueryParameters>(context), IUserProgressRepository
{
    public async Task<UserProgress?> GetByUserIdAsync(int userId, bool track = true, CancellationToken cancellationToken = default)
    {
        IQueryable<UserProgress> query = this.Context.UserProgress;

        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}
