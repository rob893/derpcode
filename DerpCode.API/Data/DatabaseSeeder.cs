using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

    public void SeedDatabase(bool seedData, bool clearCurrentData, bool applyMigrations, bool dropDatabase)
    {
        if (dropDatabase)
        {
            this.context.Database.EnsureDeleted();
        }

        if (applyMigrations)
        {
            this.context.Database.Migrate();
        }

        if (clearCurrentData)
        {
            this.ClearAllData();
        }

        if (seedData)
        {
            this.SeedRoles();
            this.SeedProblems();
            this.SeedDriverTemplates();

            this.context.SaveChanges();
        }
    }

    private void ClearAllData()
    {
        this.context.RefreshTokens.Clear();
        this.context.Problems.Clear();
        this.context.DriverTemplates.Clear();
        this.context.Users.Clear();
        this.context.Roles.Clear();

        this.context.SaveChanges();
    }

    private void SeedRoles()
    {
        if (this.context.Roles.Any())
        {
            return;
        }

        var data = File.ReadAllText("Data/SeedData/RoleSeedData.json");
        var roles = JsonSerializer.Deserialize<List<Role>>(data, jsonOptions) ?? throw new JsonException("Unable to deserialize data.");

        foreach (var role in roles)
        {
            this.roleManager.CreateAsync(role).Wait();
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