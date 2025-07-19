using System;
using System.Collections.Generic;
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
using DerpCode.API.Services.Core;
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

    private readonly IProblemSeedDataService problemSeedDataService;

    private readonly ILogger<DatabaseSeeder> logger;

    private readonly IProblemService problemService;

    private readonly ICurrentUserService currentUserService;

    public DatabaseSeeder(
        DataContext context,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IProblemSeedDataService problemSeedDataService,
        ILogger<DatabaseSeeder> logger,
        IProblemService problemService,
        ICurrentUserService currentUserService)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        this.problemSeedDataService = problemSeedDataService ?? throw new ArgumentNullException(nameof(problemSeedDataService));
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
        var folderProblems = await this.problemSeedDataService.LoadProblemsFromFolderAsync(cancellationToken);

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

            if (ProblemService.HasProblemChanged(dbProblem, folderProblem))
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
}