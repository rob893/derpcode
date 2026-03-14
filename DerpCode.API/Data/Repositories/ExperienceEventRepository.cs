using System;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public sealed class ExperienceEventRepository : Repository<ExperienceEvent, long, CursorPaginationQueryParameters>, IExperienceEventRepository
{
    public ExperienceEventRepository(DataContext context) : base(
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
