using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Entities;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Services.Core;

public sealed class CodeExecutionService : ICodeExecutionService
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly TimeSpan defaultExecutionWaitTimeout = TimeSpan.FromSeconds(60);

    // 0777 — runner UID inside the container is unlikely to match the API host UID, so the
    // submission tempDir must be writable for any UID to receive results.json/output.txt/error.txt.
    private const UnixFileMode SubmissionDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
        UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
        UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;

    private readonly IDockerClient dockerClient;

    private readonly ILogger<CodeExecutionService> logger;

    private readonly IFileSystemService fileSystemService;

    private readonly TimeSpan executionWaitTimeout;

    public CodeExecutionService(IDockerClient dockerClient, ILogger<CodeExecutionService> logger, IFileSystemService fileSystemService)
        : this(dockerClient, logger, fileSystemService, defaultExecutionWaitTimeout)
    {
    }

    public CodeExecutionService(IDockerClient dockerClient, ILogger<CodeExecutionService> logger, IFileSystemService fileSystemService, TimeSpan executionWaitTimeout)
    {
        this.dockerClient = dockerClient ?? throw new ArgumentNullException(nameof(dockerClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));

        if (executionWaitTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(executionWaitTimeout), "Execution wait timeout must be greater than zero.");
        }

        this.executionWaitTimeout = executionWaitTimeout;
    }

    public async Task<(ProblemSubmission Submission, string StdOut)> RunCodeAsync(int userId, string userCode, LanguageType language, Problem problem, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userCode);
        ArgumentNullException.ThrowIfNull(problem);

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == language) ?? throw new InvalidOperationException($"No driver found for language: {language}");

        var tempDir = this.fileSystemService.CombinePaths(this.fileSystemService.GetTempPath(), $"submission_{Guid.NewGuid()}");
        this.fileSystemService.CreateDirectory(tempDir);
        this.fileSystemService.SetUnixFileMode(tempDir, SubmissionDirectoryMode);

        try
        {
            await this.PrepareFilesAsync(tempDir, userCode, driver, problem, cancellationToken).ConfigureAwait(false);
            return await this.ExecuteInContainerAsync(userId, userCode, language, problem.Id, tempDir, driver.Image, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw; // Let cancellation exceptions bubble up
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code");
            return (new ProblemSubmission
            {
                Pass = false,
                ErrorMessage = ex.Message,
                ExecutionTimeInMs = -1,
                FailedTestCases = -1,
                PassedTestCases = -1,
                TestCaseCount = -1,
                Language = language,
                Code = userCode,
                CreatedAt = DateTimeOffset.UtcNow,
                TestCaseResults = [],
                UserId = userId,
                ProblemId = problem.Id,
            }, string.Empty);
        }
        finally
        {
            if (this.fileSystemService.DirectoryExists(tempDir))
            {
                this.fileSystemService.DeleteDirectory(tempDir, true);
            }
        }
    }

    private async Task PrepareFilesAsync(string tempDir, string userCode, ProblemDriver driver, Problem problem, CancellationToken cancellationToken)
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
            await this.fileSystemService.WriteAllTextAsync(this.fileSystemService.CombinePaths(tempDir, fileName), content, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<(ProblemSubmission Submission, string StdOut)> ExecuteInContainerAsync(
        int userId,
        string userCode,
        LanguageType language,
        int problemId,
        string tempDir,
        string image,
        CancellationToken cancellationToken)
    {
        var hostConfig = new HostConfig
        {
            Binds = [$"{tempDir}:/home/runner/submission:rw"],
            Memory = 512L * 1024 * 1024, // 512MB
            MemorySwap = 512L * 1024 * 1024, // 512MB
            CPUPercent = 50,
            AutoRemove = true,
            PidsLimit = 512,
            CapDrop = ["ALL"],
            SecurityOpt = ["no-new-privileges:true"],
            Ulimits =
            [
                new Ulimit { Name = "nofile", Soft = 1024, Hard = 1024 },
                new Ulimit { Name = "fsize", Soft = 64L * 1024 * 1024, Hard = 64L * 1024 * 1024 }
            ]
        };

        var container = await this.dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = image,
            HostConfig = hostConfig,
            NetworkDisabled = true
        }, cancellationToken).ConfigureAwait(false);

        await this.dockerClient.Containers.StartContainerAsync(container.ID, null, cancellationToken).ConfigureAwait(false);

        using var timeoutCts = new CancellationTokenSource(this.executionWaitTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await this.dockerClient.Containers.WaitContainerAsync(container.ID, linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            this.logger.LogWarning(
                "Container {ContainerId} exceeded the execution wait timeout of {TimeoutSeconds}s; forcibly stopping.",
                container.ID,
                this.executionWaitTimeout.TotalSeconds);

            await this.TryStopAndRemoveContainerAsync(container.ID).ConfigureAwait(false);

            throw new TimeoutException(
                $"Code execution exceeded the maximum allowed time of {this.executionWaitTimeout.TotalSeconds:0} seconds.");
        }
        catch (OperationCanceledException)
        {
            await this.TryStopAndRemoveContainerAsync(container.ID).ConfigureAwait(false);
            throw;
        }

        var resultsPath = this.fileSystemService.CombinePaths(tempDir, "results.json");
        var errorPath = this.fileSystemService.CombinePaths(tempDir, "error.txt");
        var outputPath = this.fileSystemService.CombinePaths(tempDir, "output.txt");

        this.logger.LogDebug("Results path: {ResultsPath}", resultsPath);

        var output = this.fileSystemService.FileExists(outputPath) ? await this.fileSystemService.ReadAllTextAsync(outputPath, cancellationToken) : string.Empty;
        var error = this.fileSystemService.FileExists(errorPath) ? await this.fileSystemService.ReadAllTextAsync(errorPath, cancellationToken) : string.Empty;

        this.logger.LogDebug("Output: {OutPut}", string.IsNullOrWhiteSpace(output) ? "No output." : output);

        if (!string.IsNullOrEmpty(error))
        {
            this.logger.LogError("Error executing code: {Error}", error);

            return (new ProblemSubmission
            {
                Pass = false,
                ErrorMessage = $"{error}\n{output}",
                ExecutionTimeInMs = -1,
                FailedTestCases = -1,
                PassedTestCases = -1,
                TestCaseCount = -1,
                Language = language,
                Code = userCode,
                CreatedAt = DateTimeOffset.UtcNow,
                TestCaseResults = [],
                UserId = userId,
                ProblemId = problemId,
            }, output);
        }

        if (!this.fileSystemService.FileExists(resultsPath))
        {
            return (new ProblemSubmission
            {
                Pass = false,
                ErrorMessage = "Failed to deserialize results",
                ExecutionTimeInMs = -1,
                FailedTestCases = -1,
                PassedTestCases = -1,
                TestCaseCount = -1,
                Language = language,
                Code = userCode,
                CreatedAt = DateTimeOffset.UtcNow,
                TestCaseResults = [],
                UserId = userId,
                ProblemId = problemId,
            }, output);
        }

        var results = await this.fileSystemService.ReadAllTextAsync(resultsPath, cancellationToken).ConfigureAwait(false);
        var submissionResult = JsonSerializer.Deserialize<ProblemSubmission>(results, jsonOptions);

        if (submissionResult == null)
        {
            this.logger.LogError("Failed to deserialize results: {Results}", results);
            throw new InvalidOperationException("Failed to deserialize results");
        }

        submissionResult.Language = language;
        submissionResult.Code = userCode;
        submissionResult.CreatedAt = DateTimeOffset.UtcNow;
        submissionResult.UserId = userId;
        submissionResult.ProblemId = problemId;

        return (submissionResult, output);
    }

    private async Task TryStopAndRemoveContainerAsync(string containerId)
    {
        // Use a fresh, short-lived cleanup token so we don't reuse a canceled token.
        using var cleanupCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            await this.dockerClient.Containers.StopContainerAsync(
                containerId,
                new ContainerStopParameters { WaitBeforeKillSeconds = 1 },
                cleanupCts.Token).ConfigureAwait(false);
        }
        catch (DockerContainerNotFoundException)
        {
            // AutoRemove may have already cleaned the container up — nothing more to do.
            return;
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Failed to stop runaway container {ContainerId}", containerId);
        }

        try
        {
            await this.dockerClient.Containers.RemoveContainerAsync(
                containerId,
                new ContainerRemoveParameters { Force = true },
                cleanupCts.Token).ConfigureAwait(false);
        }
        catch (DockerContainerNotFoundException)
        {
            // Auto-remove already handled it.
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "Failed to remove runaway container {ContainerId}", containerId);
        }
    }
}