using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public sealed class UserPreferencesRepository(DataContext context) : Repository<UserPreferences, CursorPaginationQueryParameters>(context), IUserPreferencesRepository
{ }