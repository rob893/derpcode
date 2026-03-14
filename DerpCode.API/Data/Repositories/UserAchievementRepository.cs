using System;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

/// <summary>
/// Repository for user achievement entities.
/// </summary>
public sealed class UserAchievementRepository : Repository<UserAchievement, long, CursorPaginationQueryParameters>, IUserAchievementRepository
{
    public UserAchievementRepository(DataContext context) : base(
        context,
        id => id.ConvertToBase64UrlEncodedString(),
        str =>
        {
            try
            {
                return str.ConvertToLongFromBase64UrlEncodedString();
            }
            catch
            {
                throw new ArgumentException($"{str} is not a valid base 64 encoded int64.");
            }
        })
    { }
}
