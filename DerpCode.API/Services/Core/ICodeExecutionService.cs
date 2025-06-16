using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Services.Core;

public interface ICodeExecutionService
{
    Task<ProblemSubmission> RunCodeAsync(int userId, string userCode, LanguageType language, Problem problem, CancellationToken cancellationToken);
}