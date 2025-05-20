
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public sealed class DriverTemplateRepository(DataContext context) : Repository<DriverTemplate, CursorPaginationQueryParameters>(context), IDriverTemplateRepository
{
}