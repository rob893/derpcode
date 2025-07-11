using System.Threading;
using System.Threading.Tasks;

namespace DerpCode.API.Data;

public interface IDatabaseSeeder
{
    Task SeedDatabaseAsync(bool seedData, bool clearCurrentData, bool applyMigrations, bool dropDatabase, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes problems in the database with the problems defined in the SeedData/Problems folder structure.
    /// Adds new problems, removes obsolete ones, and updates existing problems with changes from the folder.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SyncProblemsFromFolderAsync(CancellationToken cancellationToken = default);
}