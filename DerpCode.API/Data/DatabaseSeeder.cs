using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Data.SeedData;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Data;

public sealed class DatabaseSeeder : IDatabaseSeeder
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly DataContext context;

    private readonly RoleManager<Role> roleManager;

    public DatabaseSeeder(DataContext context, RoleManager<Role> roleManager)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
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

            this.SeedProblems();
            this.SeedDriverTemplates();

            await this.context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ClearAllDataAsync(CancellationToken cancellationToken = default)
    {
        this.context.RefreshTokens.Clear();
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

    private void SeedProblems()
    {
        if (this.context.Problems.Any())
        {
            return;
        }

        var problems = ProblemData.Problems;

        foreach (var problem in problems)
        {
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