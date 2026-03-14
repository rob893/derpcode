using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Data.Repositories;

public sealed class UserProblemProgressRepository : Repository<UserProblemProgress, string, CursorPaginationQueryParameters>, IUserProblemProgressRepository
{
    public UserProblemProgressRepository(DataContext context) : base(
        context,
        id => id.ConvertToBase64UrlEncodedString(),
        str =>
        {
            try
            {
                return str.ConvertToStringFromBase64UrlEncodedString();
            }
            catch
            {
                throw new ArgumentException($"{str} is not a valid base 64 encoded string.");
            }
        })
    { }

    public async Task<UserProblemProgress?> GetByUserAndProblemAsync(int userId, int problemId, bool track = true, CancellationToken cancellationToken = default)
    {
        IQueryable<UserProblemProgress> query = this.Context.UserProblemProgress;

        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(x => x.UserId == userId && x.ProblemId == problemId, cancellationToken);
    }
}
