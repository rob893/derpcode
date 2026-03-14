using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

/// <summary>
/// Repository for user achievement entities.
/// </summary>
public interface IUserAchievementRepository : IRepository<UserAchievement, long, CursorPaginationQueryParameters>
{
}
