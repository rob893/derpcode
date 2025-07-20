using System.Threading;
using System.Threading.Tasks;

namespace DerpCode.API.Services.Integrations;

public interface IGitHubService
{
    Task<string> SyncProblemsFromDatabaseToGithubAsync(CancellationToken cancellationToken = default);
}