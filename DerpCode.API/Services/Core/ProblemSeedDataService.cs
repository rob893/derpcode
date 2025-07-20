using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Services.Core;

public sealed partial class ProblemSeedDataService : IProblemSeedDataService
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        },
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles

    };

    private readonly IFileSystemService fileSystemService;

    private readonly IProblemRepository problemRepository;

    private readonly ILogger<ProblemSeedDataService> logger;

    public ProblemSeedDataService(IFileSystemService fileSystemService, IProblemRepository problemRepository, ILogger<ProblemSeedDataService> logger)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
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

    public async Task<(Dictionary<string, string> Updated, Dictionary<string, string> NewItems, HashSet<string> Deleted)> GetUpdatedProblemsToSyncFromDatabaseToGitAsync(CancellationToken cancellationToken = default)
    {
        var problemsFromDb = await this.problemRepository.SearchAsync(
               p => true,
               [p => p.ExplanationArticle, p => p.Tags, p => p.Drivers],
               track: false,
               cancellationToken);
        var problemsFromFiles = await this.LoadProblemsFromFolderAsync(cancellationToken);

        var dbProblemsAsSeedFilesLookup = problemsFromDb.ToDictionary(x => x.Id, this.ConvertProblemToSeedDataFiles);
        var problemsFromFilesAsSeedFilesLookup = problemsFromFiles.ToDictionary(x => x.Id, this.ConvertProblemToSeedDataFiles);

        var updated = new Dictionary<string, string>();
        var newItems = new Dictionary<string, string>();
        var deletedItems = problemsFromFilesAsSeedFilesLookup.Values
            .SelectMany(x => x.Keys)
            .Except(dbProblemsAsSeedFilesLookup.Values.SelectMany(x => x.Keys))
            .ToHashSet();

        foreach (var dbProblem in dbProblemsAsSeedFilesLookup)
        {
            var dbProblemFiles = dbProblem.Value;
            var problemId = dbProblem.Key;

            if (problemsFromFilesAsSeedFilesLookup.TryGetValue(problemId, out var filesProblemFiles))
            {
                foreach (var kvp in dbProblemFiles)
                {
                    if (deletedItems.Contains(kvp.Key))
                    {
                        throw new InvalidOperationException($"File {kvp.Key} was marked both for delete while being in db files!");
                    }

                    if (filesProblemFiles.TryGetValue(kvp.Key, out var filesProblemContent))
                    {
                        if (kvp.Value != filesProblemContent)
                        {
                            // Existing file changed.
                            updated[kvp.Key] = kvp.Value;
                        }
                    }
                    else
                    {
                        // New file for problem (like a new driver).
                        newItems[kvp.Key] = kvp.Value;
                    }
                }
            }
            else
            {
                // Means new problem
                foreach (var kvp in dbProblemFiles)
                {
                    if (deletedItems.Contains(kvp.Key))
                    {
                        throw new InvalidOperationException($"File {kvp.Key} was marked both for delete while being in db files!");
                    }

                    if (newItems.ContainsKey(kvp.Key))
                    {
                        throw new InvalidOperationException($"There is already a file with path ${kvp.Key} in newItems!");
                    }

                    newItems[kvp.Key] = kvp.Value;
                }
            }
        }

        return (updated, newItems, deletedItems);
    }

    private Dictionary<string, string> ConvertProblemToSeedDataFiles(Problem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(problem.ExplanationArticle);

        if (problem.Drivers == null || problem.Drivers.Count == 0)
        {
            throw new ArgumentException("Problem must have at least one driver.", nameof(problem));
        }

        var problemPath = this.fileSystemService.CombinePaths(this.GetProblemsDirectoryPath(), RemoveWhitespaceRegex().Replace(problem.Name, "") + "-" + problem.Id);

        var seedDataFiles = new Dictionary<string, string>();

        foreach (var driver in problem.Drivers)
        {
            var driverPath = this.fileSystemService.CombinePaths(problemPath, "Drivers", driver.Language.ToString());

            seedDataFiles[this.fileSystemService.CombinePaths(driverPath, "Answer.txt")] = driver.Answer;
            seedDataFiles[this.fileSystemService.CombinePaths(driverPath, "DriverCode.txt")] = driver.DriverCode;
            seedDataFiles[this.fileSystemService.CombinePaths(driverPath, "UITemplate.txt")] = driver.UITemplate;

            driver.Answer = string.Empty;
            driver.DriverCode = string.Empty;
            driver.UITemplate = string.Empty;
        }

        seedDataFiles[this.fileSystemService.CombinePaths(problemPath, "Explanation.md")] = problem.ExplanationArticle.Content;

        problem.ExplanationArticle.Content = string.Empty;

        seedDataFiles[this.fileSystemService.CombinePaths(problemPath, "Problem.json")] = ProblemDto.FromEntity(problem, true, true).ToJson(jsonOptions);

        return seedDataFiles;
    }

    private string GetProblemsDirectoryPath()
    {
        return this.fileSystemService.CombinePaths("DerpCode.API", "Data", "SeedData", "Problems");
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

    [GeneratedRegex(@"\s+")]
    private static partial Regex RemoveWhitespaceRegex();
}