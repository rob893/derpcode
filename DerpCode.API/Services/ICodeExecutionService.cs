using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Services;

public interface ICodeExecutionService
{
    Task<SubmissionResult> RunCodeAsync(string userCode, LanguageType language, Problem problem, CancellationToken cancellationToken);
}