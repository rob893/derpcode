using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Services.Core;

public sealed class ProblemSeedDataService : IProblemSeedDataService
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly IFileSystemService fileSystemService;

    private readonly ILogger<ProblemSeedDataService> logger;

    public ProblemSeedDataService(IFileSystemService fileSystemService, ILogger<ProblemSeedDataService> logger)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Problem>> LoadProblemsFromFolderAsync(CancellationToken cancellationToken = default)
    {
        var problems = new List<Problem>();
        var problemsFolderPath = this.fileSystemService.CombinePaths("Data", "SeedData", "Problems");

        if (!this.fileSystemService.DirectoryExists(problemsFolderPath))
        {
            this.logger.LogWarning("Problems folder not found: {FolderPath}", problemsFolderPath);
            throw new DirectoryNotFoundException($"Problems folder not found: {problemsFolderPath}");
        }

        var problemDirectories = this.fileSystemService.GetDirectories(problemsFolderPath);

        foreach (var problemDir in problemDirectories)
        {
            try
            {
                var problem = await this.LoadProblemFromDirectoryAsync(problemDir, cancellationToken);
                problems.Add(problem);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to load problem from directory: {Directory}", problemDir);
                throw;
            }
        }

        return problems;
    }

    public async Task<Problem> LoadProblemFromDirectoryAsync(string problemDirectory, CancellationToken cancellationToken = default)
    {
        var problemJsonPath = this.fileSystemService.CombinePaths(problemDirectory, "Problem.json");
        var explanationPath = this.fileSystemService.CombinePaths(problemDirectory, "Explanation.md");
        var driversPath = this.fileSystemService.CombinePaths(problemDirectory, "Drivers");

        if (!this.fileSystemService.FileExists(problemJsonPath))
        {
            this.logger.LogWarning("Problem.json not found in: {Directory}", problemDirectory);
            throw new FileNotFoundException("Problem.json not found", problemJsonPath);
        }

        // Load problem from JSON
        var problemJson = await this.fileSystemService.ReadAllTextAsync(problemJsonPath, cancellationToken);
        var problem = JsonSerializer.Deserialize<Problem>(problemJson, jsonOptions);

        if (problem == null)
        {
            this.logger.LogWarning("Failed to deserialize problem from: {File}", problemJsonPath);
            throw new JsonException("Failed to deserialize problem from JSON.");
        }

        if (!this.fileSystemService.FileExists(explanationPath))
        {
            throw new InvalidOperationException($"No file for ${explanationPath}. Problems must have an explanation.");
        }

        var explanationContent = await this.fileSystemService.ReadAllTextAsync(explanationPath, cancellationToken);
        problem.ExplanationArticle.Content = explanationContent;

        if (!this.fileSystemService.DirectoryExists(driversPath))
        {
            throw new InvalidOperationException($"No folder for drivers at {driversPath}. Problems must have drivers.");
        }

        await this.HydrateDriversFromDirectoryAsync(problem, driversPath, cancellationToken);

        return problem;
    }

    public Dictionary<string, string> ConvertProblemToSeedDataFiles(Problem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(problem.ExplanationArticle);

        if (problem.Drivers == null || problem.Drivers.Count == 0)
        {
            throw new ArgumentException("Problem must have at least one driver.", nameof(problem));
        }

        var clone = problem.JsonClone();
        var problemPath = this.fileSystemService.CombinePaths(this.GetProblemsDirectoryPath(), clone.Name);

        var seedDataFiles = new Dictionary<string, string>();

        foreach (var driver in clone.Drivers)
        {
            var driverPath = this.fileSystemService.CombinePaths(problemPath, "Drivers", driver.Language.ToString());

            seedDataFiles[this.fileSystemService.CombinePaths(driverPath, "Answer.txt")] = driver.Answer;
            seedDataFiles[this.fileSystemService.CombinePaths(driverPath, "DriverCode.txt")] = driver.DriverCode;
            seedDataFiles[this.fileSystemService.CombinePaths(driverPath, "UITemplate.txt")] = driver.UITemplate;
        }

        seedDataFiles[this.fileSystemService.CombinePaths(problemPath, "Explanation.md")] = clone.ExplanationArticle.Content;

        clone.Drivers = [];
        clone.ExplanationArticle.Content = string.Empty;

        return seedDataFiles;
    }

    // public async Task SaveProblemToSeedDataAsync(Problem problem)
    // {
    //     ArgumentNullException.ThrowIfNull(problem);
    //     ArgumentNullException.ThrowIfNull(problem.ExplanationArticle);

    //     if (problem.Drivers == null || problem.Drivers.Count == 0)
    //     {
    //         throw new ArgumentException("Problem must have at least one driver.", nameof(problem));
    //     }
    // }

    private string GetProblemsDirectoryPath()
    {
        return this.fileSystemService.CombinePaths(this.fileSystemService.GetCurrentDirectory(), "Data", "SeedData", "Problems");
    }

    private async Task HydrateDriversFromDirectoryAsync(Problem problem, string driversDirectory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (problem.Drivers == null || problem.Drivers.Count == 0)
        {
            throw new ArgumentException("Problem must have at least 1 driver.");
        }

        foreach (var driver in problem.Drivers)
        {
            var driverContentPath = this.fileSystemService.CombinePaths(driversDirectory, driver.Language.ToString());

            if (!this.fileSystemService.DirectoryExists(driverContentPath))
            {
                throw new InvalidOperationException($"No driver folder found at {driverContentPath}.");
            }

            await this.HydrateDriverFromDirectoryAsync(driverContentPath, problem.Id, driver, cancellationToken);
        }
    }

    private async Task HydrateDriverFromDirectoryAsync(string driverDirectory, int problemId, ProblemDriver driver, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(driver);

        var driverCodePath = this.fileSystemService.CombinePaths(driverDirectory, "DriverCode.txt");
        var uiTemplatePath = this.fileSystemService.CombinePaths(driverDirectory, "UITemplate.txt");
        var answerPath = this.fileSystemService.CombinePaths(driverDirectory, "Answer.txt");

        if (!this.fileSystemService.FileExists(driverCodePath) ||
            !this.fileSystemService.FileExists(uiTemplatePath) ||
            !this.fileSystemService.FileExists(answerPath))
        {
            this.logger.LogWarning("Missing driver files in: {Directory}", driverDirectory);
            throw new FileNotFoundException("Driver files not found", driverDirectory);
        }

        var driverCode = await this.fileSystemService.ReadAllTextAsync(driverCodePath, cancellationToken);
        var uiTemplate = await this.fileSystemService.ReadAllTextAsync(uiTemplatePath, cancellationToken);
        var answer = await this.fileSystemService.ReadAllTextAsync(answerPath, cancellationToken);

        driver.Answer = answer;
        driver.UITemplate = uiTemplate;
        driver.DriverCode = driverCode;
        driver.ProblemId = problemId;
    }
}