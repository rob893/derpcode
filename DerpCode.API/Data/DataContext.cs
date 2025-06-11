using System;
using System.Collections.Generic;
using System.Text.Json;
using DerpCode.API.Data.Comparers;
using DerpCode.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Data;

public sealed class DataContext : IdentityDbContext<User, Role, int,
    IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
    IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => this.Set<RefreshToken>();

    public DbSet<LinkedAccount> LinkedAccounts => this.Set<LinkedAccount>();

    public DbSet<Problem> Problems => this.Set<Problem>();

    public DbSet<ProblemDriver> ProblemDrivers => this.Set<ProblemDriver>();

    public DbSet<DriverTemplate> DriverTemplates => this.Set<DriverTemplate>();

    public DbSet<Tag> Tags => this.Set<Tag>();

    public DbSet<ProblemSubmission> ProblemSubmissions => this.Set<ProblemSubmission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        builder.Entity<UserRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            userRole.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        builder.Entity<RefreshToken>(rToken =>
        {
            rToken.HasKey(k => new { k.UserId, k.DeviceId });
        });

        builder.Entity<LinkedAccount>(linkedAccount =>
        {
            linkedAccount.HasKey(account => new { account.Id, account.LinkedAccountType });
            linkedAccount.Property(account => account.LinkedAccountType).HasConversion<string>();
        });

        builder.Entity<ProblemDriver>(driver =>
        {
            driver.Property(d => d.Language).HasConversion<string>();
        });

        builder.Entity<DriverTemplate>(driver =>
        {
            driver.Property(d => d.Language).HasConversion<string>();
        });

        builder.Entity<Problem>(problem =>
        {
            problem.Property(p => p.Difficulty).HasConversion<string>();

            // Use JSON serialization for MySQL
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Create a comparer for JSON collections
            var listObjectComparer = new JsonCollectionComparer<List<object>>(options);
            var listStringComparer = new JsonCollectionComparer<List<string>>(options);

            problem.Property(p => p.ExpectedOutput)
                .HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, options),
                    v => JsonSerializer.Deserialize<List<object>>(v, options) ?? new List<object>())
                .Metadata.SetValueComparer(listObjectComparer);

            problem.Property(p => p.Input)
                .HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, options),
                    v => JsonSerializer.Deserialize<List<object>>(v, options) ?? new List<object>())
                .Metadata.SetValueComparer(listObjectComparer);

            problem.Property(p => p.Hints)
                .HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, options),
                    v => JsonSerializer.Deserialize<List<string>>(v, options) ?? new List<string>())
                .Metadata.SetValueComparer(listStringComparer);
        });

        builder.Entity<ProblemSubmission>(submission =>
        {
            submission.Property(s => s.Language).HasConversion<string>();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var listComparer = new JsonCollectionComparer<List<TestCaseResult>>(options);

            submission.Property(s => s.TestCaseResults)
                .HasColumnType("json")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, options),
                    v => JsonSerializer.Deserialize<List<TestCaseResult>>(v, options) ?? new List<TestCaseResult>())
                .Metadata.SetValueComparer(listComparer);
        });
    }
}