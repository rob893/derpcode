using System.Threading;
using System.Threading.Tasks;

namespace DerpCode.API.Data;

public interface IDatabaseSeeder
{
    Task SeedDatabaseAsync(bool seedData, bool clearCurrentData, bool applyMigrations, bool dropDatabase, CancellationToken cancellationToken = default);
}