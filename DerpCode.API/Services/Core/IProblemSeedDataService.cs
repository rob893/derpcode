using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Services.Core;

public interface IProblemSeedDataService
{
    Task<List<Problem>> LoadProblemsFromFolderAsync(CancellationToken cancellationToken = default);

    Task<Problem> LoadProblemFromDirectoryAsync(string problemDirectory, CancellationToken cancellationToken = default);
}