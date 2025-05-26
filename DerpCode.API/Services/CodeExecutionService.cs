using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models;
using DerpCode.API.Models.Entities;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Services;

public class CodeExecutionService : ICodeExecutionService
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDockerClient dockerClient;

    private readonly ILogger<CodeExecutionService> logger;

    public CodeExecutionService(IDockerClient dockerClient, ILogger<CodeExecutionService> logger)
    {
        this.dockerClient = dockerClient ?? throw new ArgumentNullException(nameof(dockerClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SubmissionResult> RunCodeAsync(string userCode, LanguageType language, Problem problem, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userCode);
        ArgumentNullException.ThrowIfNull(problem);

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == language) ?? throw new InvalidOperationException($"No driver found for language: {language}");

        var tempDir = Path.Combine(Path.GetTempPath(), $"submission_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            await PrepareFilesAsync(tempDir, userCode, driver, problem, cancellationToken).ConfigureAwait(false);
            return await this.ExecuteInContainerAsync(tempDir, driver.Image, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code");
            return new SubmissionResult
            {
                Pass = false,
                ErrorMessage = ex.Message,
                ExecutionTimeInMs = -1,
                FailedTestCases = -1,
                PassedTestCases = -1,
                TestCaseCount = -1
            };
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    private static async Task PrepareFilesAsync(string tempDir, string userCode, ProblemDriver driver, Problem problem, CancellationToken cancellationToken)
    {
        var files = new Dictionary<string, string>
        {
            { "UserCode.txt", userCode },
            { "DriverCode.txt", driver.DriverCode },
            { "input.json", JsonSerializer.Serialize(problem.Input) },
            { "expectedOutput.json", JsonSerializer.Serialize(problem.ExpectedOutput) }
        };

        foreach (var (fileName, content) in files)
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, fileName), content, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<SubmissionResult> ExecuteInContainerAsync(string tempDir, string image, CancellationToken cancellationToken)
    {
        var hostConfig = new HostConfig
        {
            Binds = [$"{tempDir}:/home/runner/submission:rw"],
            Memory = 512L * 1024 * 1024, // 512MB
            MemorySwap = 512L * 1024 * 1024, // 512MB
            CPUPercent = 50,
            AutoRemove = true
        };

        var container = await this.dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = image,
            Cmd = ["/bin/bash", "/home/runner/run.sh"],
            HostConfig = hostConfig,
            NetworkDisabled = true,
            User = "root"
        }, cancellationToken);

        await this.dockerClient.Containers.StartContainerAsync(container.ID, null, cancellationToken).ConfigureAwait(false);
        await this.dockerClient.Containers.WaitContainerAsync(container.ID, cancellationToken).ConfigureAwait(false);

        var resultsPath = Path.Combine(tempDir, "results.json");
        var errorPath = Path.Combine(tempDir, "error.txt");
        var outputPath = Path.Combine(tempDir, "output.txt");

        this.logger.LogInformation("Results path: {ResultsPath}", resultsPath);

        var output = File.Exists(outputPath) ? await File.ReadAllTextAsync(outputPath, cancellationToken) : string.Empty;
        var error = File.Exists(errorPath) ? await File.ReadAllTextAsync(errorPath, cancellationToken) : string.Empty;

        this.logger.LogInformation("Output: {OutPut}", string.IsNullOrWhiteSpace(output) ? "No output." : output);

        if (!string.IsNullOrEmpty(error))
        {
            this.logger.LogError("Error executing code: {Error}", error);

            return new SubmissionResult
            {
                Pass = false,
                ErrorMessage = $"{error}\n{output}",
                ExecutionTimeInMs = -1,
                FailedTestCases = -1,
                PassedTestCases = -1,
                TestCaseCount = -1
            };
        }

        var results = await File.ReadAllTextAsync(resultsPath, cancellationToken).ConfigureAwait(false);
        var submissionResult = JsonSerializer.Deserialize<SubmissionResult>(results, jsonOptions);

        return submissionResult ?? throw new InvalidOperationException("Failed to deserialize results");
    }
}