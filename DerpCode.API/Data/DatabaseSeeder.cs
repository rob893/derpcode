using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Data.SeedData;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Requests;
using DerpCode.API.Services.Auth;
using DerpCode.API.Services.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Data;

public sealed class DatabaseSeeder : IDatabaseSeeder
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly DataContext context;

    private readonly UserManager<User> userManager;

    private readonly RoleManager<Role> roleManager;

    private readonly ILogger<DatabaseSeeder> logger;

    private readonly IProblemService problemService;

    private readonly ICurrentUserService currentUserService;

    public DatabaseSeeder(DataContext context, UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<DatabaseSeeder> logger, IProblemService problemService, ICurrentUserService currentUserService)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.problemService = problemService ?? throw new ArgumentNullException(nameof(problemService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task SeedDatabaseAsync(bool seedData, bool clearCurrentData, bool applyMigrations, bool dropDatabase, CancellationToken cancellationToken = default)
    {
        if (dropDatabase)
        {
            await this.context.Database.EnsureDeletedAsync(cancellationToken);
        }

        if (applyMigrations)
        {
            await this.context.Database.MigrateAsync(cancellationToken);
        }

        if (clearCurrentData)
        {
            await this.ClearAllDataAsync(cancellationToken);
        }

        if (seedData)
        {
            await this.SeedRolesAsync(cancellationToken);
            await this.SeedUsersAsync(cancellationToken);

            //this.SeedProblems(); // remove this once I test the folder sync functionality
            this.SeedDriverTemplates();

            await this.context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ClearAllDataAsync(CancellationToken cancellationToken = default)
    {
        this.context.RefreshTokens.Clear();
        this.context.LinkedAccounts.Clear();
        this.context.ArticleComments.Clear();
        this.context.Articles.Clear();
        this.context.ProblemSubmissions.Clear();
        this.context.ProblemDrivers.Clear();
        this.context.Tags.Clear();
        this.context.Problems.Clear();
        this.context.DriverTemplates.Clear();
        this.context.Users.Clear();
        this.context.Roles.Clear();

        await this.context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken = default)
    {
        if (this.context.Roles.Any())
        {
            return;
        }

        var data = await File.ReadAllTextAsync("Data/SeedData/RoleSeedData.json", cancellationToken);
        var roles = JsonSerializer.Deserialize<List<Role>>(data, jsonOptions) ?? throw new JsonException("Unable to deserialize data.");

        foreach (var role in roles)
        {
            await this.roleManager.CreateAsync(role);
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken = default)
    {
        if (this.context.Users.Any())
        {
            return;
        }

        var newUser = new User
        {
            UserName = "rob893",
            Email = "rwherber@gmail.com",
            EmailConfirmed = true,
            Id = 1,
            Created = DateTimeOffset.UtcNow,
            LastPasswordChange = DateTimeOffset.UtcNow,
            LastEmailChange = DateTimeOffset.UtcNow,
            LastUsernameChange = DateTimeOffset.UtcNow,
        };

        await this.userManager.CreateAsync(newUser);
        await this.userManager.AddToRoleAsync(newUser, UserRoleName.User);
        await this.userManager.AddToRoleAsync(newUser, UserRoleName.Admin);
        await this.userManager.AddToRoleAsync(newUser, UserRoleName.PremiumUser);
    }

    private void SeedProblems()
    {
        if (this.context.Problems.Any())
        {
            return;
        }

        var problems = ProblemData.Problems;

        foreach (var problem in problems)
        {
            for (int i = 0; i < 15; i++)
            {
                problem.SolutionArticles.Add(new Article
                {
                    UserId = 1,
                    Title = $"Solution {i + 1}",
                    Content = "This is a solution article.",
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    LastEditedById = 1,
                    Type = ArticleType.ProblemSolution
                });
            }
            this.context.Problems.Add(problem);
        }
    }

    private void SeedDriverTemplates()
    {
        if (this.context.DriverTemplates.Any())
        {
            return;
        }

        var driverTemplates = DriverTemplateData.Templates;

        foreach (var template in driverTemplates)
        {
            this.context.DriverTemplates.Add(template);
        }
    }

    /// <summary>
    /// Synchronizes problems in the database with the problems defined in the SeedData/Problems folder structure.
    /// Adds new problems, removes obsolete ones, and updates existing problems with changes from the folder.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SyncProblemsFromFolderAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Starting problem synchronization from folder structure...");

        // Fetch user with ID 1 from database and override current user service
        var systemUser = await this.context.Users.FirstOrDefaultAsync(u => u.Id == 1, cancellationToken)
            ?? throw new InvalidOperationException("System user with ID 1 not found in database. Cannot proceed with problem synchronization.");
        this.currentUserService.SetOverrideUser(systemUser);

        // Load all problems and their drivers from the database
        var dbProblems = await this.context.Problems
            .Include(p => p.Drivers)
            .Include(p => p.Tags)
            .Include(p => p.ExplanationArticle)
            .ToListAsync(cancellationToken);

        // Load all problems from the folder structure
        var folderProblems = await this.LoadProblemsFromFolderAsync(cancellationToken);

        // Create dictionaries for easier comparison
        var dbProblemsDict = dbProblems.ToDictionary(p => p.Id);
        var folderProblemsDict = folderProblems.ToDictionary(p => p.Id);

        // Find problems to add (exist in folder but not in database)
        var problemsToAdd = folderProblems.Where(fp => !dbProblemsDict.ContainsKey(fp.Id)).ToList();

        // Find problems to remove (exist in database but not in folder)
        var problemsToRemove = dbProblems.Where(dp => !folderProblemsDict.ContainsKey(dp.Id)).ToList();

        // Find problems to update (exist in both but may have changes)
        var problemsToUpdate = folderProblems.Where(fp => dbProblemsDict.ContainsKey(fp.Id)).ToList();

        var addedCount = 0;
        var removedCount = 0;
        var updatedCount = 0;

        // Add new problems using ProblemService with specified IDs
        foreach (var problem in problemsToAdd)
        {
            this.logger.LogInformation("Adding new problem: {ProblemName} (ID: {ProblemId})", problem.Name, problem.Id);
            var createRequest = CreateProblemRequest.FromEntity(problem);
            var result = await this.problemService.CreateProblemAsync(createRequest, problem.Id, cancellationToken);

            if (result.IsSuccess)
            {
                addedCount++;
            }
            else
            {
                this.logger.LogError("Failed to add problem {ProblemName} (ID: {ProblemId}): {Error}", problem.Name, problem.Id, result.ErrorMessage);
            }
        }

        // Remove obsolete problems using ProblemService
        foreach (var problem in problemsToRemove)
        {
            this.logger.LogInformation("Removing obsolete problem: {ProblemName} (ID: {ProblemId})", problem.Name, problem.Id);
            var result = await this.problemService.DeleteProblemAsync(problem.Id, cancellationToken);

            if (result.IsSuccess)
            {
                removedCount++;
            }
            else
            {
                this.logger.LogError("Failed to remove problem {ProblemName} (ID: {ProblemId}): {Error}", problem.Name, problem.Id, result.ErrorMessage);
            }
        }

        // Update existing problems using ProblemService
        foreach (var folderProblem in problemsToUpdate)
        {
            var dbProblem = dbProblemsDict[folderProblem.Id];

            if (HasProblemChanged(dbProblem, folderProblem))
            {
                this.logger.LogInformation("Updating problem: {ProblemName} (ID: {ProblemId})", folderProblem.Name, folderProblem.Id);
                var updateRequest = CreateProblemRequest.FromEntity(folderProblem);
                var result = await this.problemService.UpdateProblemAsync(folderProblem.Id, updateRequest, cancellationToken);

                if (result.IsSuccess)
                {
                    updatedCount++;
                }
                else
                {
                    this.logger.LogError("Failed to update problem {ProblemName} (ID: {ProblemId}): {Error}", folderProblem.Name, folderProblem.Id, result.ErrorMessage);
                }
            }
        }

        this.logger.LogInformation("Problem synchronization completed. Added: {AddedCount}, Removed: {RemovedCount}, Updated: {UpdatedCount}",
            addedCount, removedCount, updatedCount);

        this.currentUserService.ClearOverrideUser();
    }

    private async Task<List<Problem>> LoadProblemsFromFolderAsync(CancellationToken cancellationToken = default)
    {
        var problems = new List<Problem>();
        var problemsFolderPath = Path.Combine("Data", "SeedData", "Problems");

        if (!Directory.Exists(problemsFolderPath))
        {
            this.logger.LogWarning("Problems folder not found: {FolderPath}", problemsFolderPath);
            throw new DirectoryNotFoundException($"Problems folder not found: {problemsFolderPath}");
        }

        var problemDirectories = Directory.GetDirectories(problemsFolderPath);

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

    private async Task<Problem> LoadProblemFromDirectoryAsync(string problemDirectory, CancellationToken cancellationToken = default)
    {
        var problemJsonPath = Path.Combine(problemDirectory, "Problem.json");
        var explanationPath = Path.Combine(problemDirectory, "Explanation.md");
        var driversPath = Path.Combine(problemDirectory, "Drivers");

        if (!File.Exists(problemJsonPath))
        {
            this.logger.LogWarning("Problem.json not found in: {Directory}", problemDirectory);
            throw new FileNotFoundException("Problem.json not found", problemJsonPath);
        }

        // Load problem from JSON
        var problemJson = await File.ReadAllTextAsync(problemJsonPath, cancellationToken);
        var problem = JsonSerializer.Deserialize<Problem>(problemJson, jsonOptions);

        if (problem == null)
        {
            this.logger.LogWarning("Failed to deserialize problem from: {File}", problemJsonPath);
            throw new JsonException("Failed to deserialize problem from JSON.");
        }

        // Load explanation content if it exists
        if (File.Exists(explanationPath))
        {
            var explanationContent = await File.ReadAllTextAsync(explanationPath, cancellationToken);
            problem.ExplanationArticle.Content = explanationContent;
        }

        // Load drivers if directory exists
        if (Directory.Exists(driversPath))
        {
            var drivers = await this.LoadDriversFromDirectoryAsync(driversPath, problem.Id, cancellationToken);
            problem.Drivers = drivers;
        }

        return problem;
    }

    private async Task<List<ProblemDriver>> LoadDriversFromDirectoryAsync(string driversDirectory, int problemId, CancellationToken cancellationToken = default)
    {
        var drivers = new List<ProblemDriver>();
        var languageDirectories = Directory.GetDirectories(driversDirectory);

        foreach (var languageDir in languageDirectories)
        {
            var languageName = Path.GetFileName(languageDir);

            if (!Enum.TryParse<LanguageType>(languageName, true, out var languageType))
            {
                this.logger.LogWarning("Unknown language type: {Language}", languageName);
                throw new InvalidOperationException($"Unknown language type: {languageName}");
            }

            try
            {
                var driver = await this.LoadDriverFromDirectoryAsync(languageDir, problemId, languageType, cancellationToken);
                drivers.Add(driver);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to load driver for language: {Language}", languageName);
                throw;
            }
        }

        return drivers;
    }

    private async Task<ProblemDriver> LoadDriverFromDirectoryAsync(string driverDirectory, int problemId, LanguageType language, CancellationToken cancellationToken = default)
    {
        var driverCodePath = Path.Combine(driverDirectory, "DriverCode.txt");
        var uiTemplatePath = Path.Combine(driverDirectory, "UITemplate.txt");
        var answerPath = Path.Combine(driverDirectory, "Answer.txt");

        if (!File.Exists(driverCodePath) || !File.Exists(uiTemplatePath) || !File.Exists(answerPath))
        {
            this.logger.LogWarning("Missing driver files in: {Directory}", driverDirectory);
            throw new FileNotFoundException("Driver files not found", driverDirectory);
        }

        var driverCode = await File.ReadAllTextAsync(driverCodePath, cancellationToken);
        var uiTemplate = await File.ReadAllTextAsync(uiTemplatePath, cancellationToken);
        var answer = await File.ReadAllTextAsync(answerPath, cancellationToken);

        return new ProblemDriver
        {
            ProblemId = problemId,
            Language = language,
            Image = language switch
            {
                LanguageType.CSharp => "code-executor-csharp",
                LanguageType.JavaScript => "code-executor-javascript",
                LanguageType.TypeScript => "code-executor-typescript",
                LanguageType.Rust => "code-executor-rust",
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Unsupported language type")
            },
            DriverCode = driverCode,
            UITemplate = uiTemplate,
            Answer = answer
        };
    }

    private static bool HasProblemChanged(Problem dbProblem, Problem folderProblem)
    {
        // Compare basic properties
        if (dbProblem.Name != folderProblem.Name ||
            dbProblem.Description != folderProblem.Description ||
            dbProblem.Difficulty != folderProblem.Difficulty ||
            dbProblem.ExplanationArticle.Content != folderProblem.ExplanationArticle.Content)
        {
            return true;
        }

        // Compare lists (Input, ExpectedOutput, Hints)
        if (!AreListsEqual(dbProblem.Input, folderProblem.Input) ||
            !AreListsEqual(dbProblem.ExpectedOutput, folderProblem.ExpectedOutput) ||
            !AreListsEqual(dbProblem.Hints, folderProblem.Hints))
        {
            return true;
        }

        // Compare tags
        if (!AreTagsEqual(dbProblem.Tags, folderProblem.Tags))
        {
            return true;
        }

        // Compare drivers
        if (!AreDriversEqual(dbProblem.Drivers, folderProblem.Drivers))
        {
            return true;
        }

        return false;
    }

    private static bool AreListsEqual<T>(List<T> list1, List<T> list2)
    {
        if (list1.Count != list2.Count)
        {
            return false;
        }

        // For List<object>, use JSON serialization to compare values as this handles
        // type differences between deserialized JSON and database objects
        if (typeof(T) == typeof(object))
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json1 = JsonSerializer.Serialize(list1, options);
            var json2 = JsonSerializer.Serialize(list2, options);
            return json1 == json2;
        }

        // For other types, use regular equality comparison
        for (int i = 0; i < list1.Count; i++)
        {
            if (!Equals(list1[i], list2[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreTagsEqual(List<Tag> dbTags, List<Tag> folderTags)
    {
        if (dbTags.Count != folderTags.Count)
        {
            return false;
        }

        var dbTagNames = dbTags.Select(t => t.Name).OrderBy(n => n).ToList();
        var folderTagNames = folderTags.Select(t => t.Name).OrderBy(n => n).ToList();

        return dbTagNames.SequenceEqual(folderTagNames);
    }

    private static bool AreDriversEqual(List<ProblemDriver> dbDrivers, List<ProblemDriver> folderDrivers)
    {
        if (dbDrivers.Count != folderDrivers.Count)
        {
            return false;
        }

        var dbDriversDict = dbDrivers.ToDictionary(d => d.Language);
        var folderDriversDict = folderDrivers.ToDictionary(d => d.Language);

        foreach (var kvp in folderDriversDict)
        {
            if (!dbDriversDict.TryGetValue(kvp.Key, out var dbDriver))
            {
                return false;
            }

            var folderDriver = kvp.Value;
            if (dbDriver.DriverCode != folderDriver.DriverCode ||
                dbDriver.UITemplate != folderDriver.UITemplate ||
                dbDriver.Answer != folderDriver.Answer ||
                dbDriver.Image != folderDriver.Image)
            {
                return false;
            }
        }

        return true;
    }
}