using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Data.SeedData;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Data;

public sealed class DatabaseSeeder : IDatabaseSeeder
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly DataContext context;

    private readonly UserManager<User> userManager;

    private readonly RoleManager<Role> roleManager;

    private readonly ILogger<DatabaseSeeder> logger;

    public DatabaseSeeder(DataContext context, UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<DatabaseSeeder> logger)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            this.SeedProblems();
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
}