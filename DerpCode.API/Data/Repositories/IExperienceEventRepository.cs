using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public interface IExperienceEventRepository : IRepository<ExperienceEvent, long, CursorPaginationQueryParameters>
{ }
