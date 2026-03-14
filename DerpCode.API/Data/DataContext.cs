using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DerpCode.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    public DbSet<UserFavoriteProblem> UserFavoriteProblems => this.Set<UserFavoriteProblem>();

    public DbSet<Tag> Tags => this.Set<Tag>();

    public DbSet<ProblemSubmission> ProblemSubmissions => this.Set<ProblemSubmission>();

    public DbSet<UserProblemProgress> UserProblemProgress => this.Set<UserProblemProgress>();

    public DbSet<UserProgress> UserProgress => this.Set<UserProgress>();

    public DbSet<ExperienceEvent> ExperienceEvents => this.Set<ExperienceEvent>();

    public DbSet<UserAchievement> UserAchievements => this.Set<UserAchievement>();

    public DbSet<Article> Articles => this.Set<Article>();

    public DbSet<ArticleComment> ArticleComments => this.Set<ArticleComment>();

    public DbSet<UserPreferences> UserPreferences => this.Set<UserPreferences>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        builder.Entity<UserFavoriteProblem>(b =>
        {
            b.HasKey(x => new { x.UserId, x.ProblemId });

            b.HasOne(x => x.User)
                .WithMany(u => u.FavoriteProblems)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Problem)
                .WithMany(p => p.FavoritedByUsers)
                .HasForeignKey(x => x.ProblemId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.CreatedAt)
                .HasDefaultValueSql("now()"); // Postgres specific
        });

        builder.Entity<UserPreferences>(preferences =>
        {
            preferences.Property(p => p.LastUpdated).HasDefaultValueSql("now()"); // Postgres specific

            var preferencesComparer = new ValueComparer<Preferences>(
                (left, right) => JsonSerializer.Serialize(left, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(right, (JsonSerializerOptions?)null),
                value => StringComparer.Ordinal.GetHashCode(JsonSerializer.Serialize(value, (JsonSerializerOptions?)null)),
                value => JsonSerializer.Deserialize<Preferences>(JsonSerializer.Serialize(value, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? new Preferences());

            preferences.Property(p => p.Preferences)
                .HasColumnType("jsonb")
                .Metadata.SetValueComparer(preferencesComparer);
        });

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

            problem.Property(p => p.ExpectedOutput).HasColumnType("jsonb");
            problem.Property(p => p.Input).HasColumnType("jsonb");
            problem.Property(p => p.Hints).HasColumnType("jsonb");
        });

        builder.Entity<ProblemSubmission>(submission =>
        {
            submission.Property(s => s.Language).HasConversion<string>();
            submission.Property(s => s.TestCaseResults).HasColumnType("jsonb");
        });

        builder.Entity<UserProgress>(progress =>
        {
            progress.HasKey(x => x.UserId);

            progress.HasOne(x => x.User)
                .WithOne(x => x.Progress)
                .HasForeignKey<UserProgress>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            progress.Property(x => x.RowVersion)
                .IsRowVersion();
        });

        builder.Entity<ExperienceEvent>(eventEntity =>
        {
            eventEntity.Property(x => x.EventType).HasConversion<string>();
            eventEntity.Property(x => x.SourceType).HasConversion<string>();
            eventEntity.Property(x => x.Metadata).HasColumnType("jsonb");

            eventEntity.HasIndex(x => x.IdempotencyKey).IsUnique();
        });

        builder.Entity<UserProblemProgress>(progress =>
        {
            progress.HasKey(x => new { x.UserId, x.ProblemId });

            progress.HasOne(x => x.User)
                .WithMany(u => u.ProblemProgress)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            progress.HasOne(x => x.Problem)
                .WithMany(p => p.UserProgress)
                .HasForeignKey(x => x.ProblemId)
                .OnDelete(DeleteBehavior.Cascade);

            progress.Property(x => x.OpenedHintIndicesCurrentCycle)
                .HasColumnType("jsonb")
                .HasConversion(
                    new ValueConverter<List<int>, string>(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>()),
                    new ValueComparer<List<int>>(
                        (left, right) => left != null && right != null && left.SequenceEqual(right),
                        v => v.Aggregate(0, (a, i) => HashCode.Combine(a, i)),
                        v => v.ToList()));
        });

        builder.Entity<UserAchievement>(achievement =>
        {
            achievement.Property(x => x.AchievementType).HasConversion<string>();

            achievement.HasOne(x => x.User)
                .WithMany(u => u.Achievements)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            achievement.HasIndex(x => new { x.UserId, x.AchievementType }).IsUnique();
        });

        builder.Entity<Article>(article =>
        {
            article.Property(s => s.Type).HasConversion<string>();
        });

        builder.Entity<ArticleComment>(comment =>
        {
            comment.HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            comment.HasOne(c => c.QuotedComment)
                .WithMany()
                .HasForeignKey(c => c.QuotedCommentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
