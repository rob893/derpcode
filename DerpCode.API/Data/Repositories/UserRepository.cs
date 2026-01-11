using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DerpCode.API.Data.Repositories;

public sealed class UserRepository : Repository<User, CursorPaginationQueryParameters>, IUserRepository
{
    public UserManager<User> UserManager { get; init; }

    private readonly SignInManager<User> signInManager;

    public UserRepository(DataContext context, UserManager<User> userManager, SignInManager<User> signInManager) : base(context)
    {
        this.UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    }

    public async Task<IdentityResult> CreateUserWithoutPasswordAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Created = DateTimeOffset.UtcNow;
        user.LastPasswordChange = DateTimeOffset.UtcNow;
        user.LastEmailChange = DateTimeOffset.UtcNow;
        user.LastUsernameChange = DateTimeOffset.UtcNow;
        var created = await this.UserManager.CreateAsync(user);

        if (!created.Succeeded)
        {
            return created;
        }

        await this.UserManager.AddToRoleAsync(user, UserRoleName.User);

        return created;
    }

    public async Task<IdentityResult> CreateUserWithPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Created = DateTimeOffset.UtcNow;
        user.LastPasswordChange = DateTimeOffset.UtcNow;
        user.LastEmailChange = DateTimeOffset.UtcNow;
        user.LastUsernameChange = DateTimeOffset.UtcNow;
        var created = await this.UserManager.CreateAsync(user, password);

        if (!created.Succeeded)
        {
            return created;
        }

        await this.UserManager.AddToRoleAsync(user, UserRoleName.User);

        return created;
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = this.Context.Users;
        query = this.AddIncludes(query);

        return query.OrderBy(e => e.Id).FirstOrDefaultAsync(user => user.UserName == username, cancellationToken);
    }

    public async Task<User?> GetByLinkedAccountAsync(string id, LinkedAccountType accountType, Expression<Func<User, object>>[] includes, CancellationToken cancellationToken = default)
    {
        var linkedAccount = await this.Context.LinkedAccounts.FirstOrDefaultAsync(account => account.Id == id && account.LinkedAccountType == accountType, cancellationToken);

        if (linkedAccount == null)
        {
            return null;
        }

        IQueryable<User> query = this.Context.Users;
        query = this.AddIncludes(query);
        query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

        return await query.OrderBy(e => e.Id).FirstOrDefaultAsync(user => user.Id == linkedAccount.UserId, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, Expression<Func<User, object>>[] includes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        IQueryable<User> query = this.Context.Users;

        query = this.AddIncludes(query);
        query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

        return query.OrderBy(e => e.Id).FirstOrDefaultAsync(user => user.UserName == username, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, Expression<Func<User, object>>[] includes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var normalizedEmail = email.ToUpperInvariant();

        IQueryable<User> query = this.Context.Users;

        query = this.AddIncludes(query);
        query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

        return query.OrderBy(e => e.Id).FirstOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await this.signInManager.CheckPasswordSignInAsync(user, password, false);

        return result.Succeeded;
    }

    public Task<CursorPaginatedList<Role, int>> GetRolesAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken = default)
    {
        IQueryable<Role> query = this.Context.Roles;

        return query.ToCursorPaginatedListAsync(searchParams, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await this.Context.Roles.ToListAsync(cancellationToken);

        return roles;
    }

    public async Task<IReadOnlyList<RefreshToken>> GetRefreshTokensForDeviceAsync(string deviceId, bool track = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        IQueryable<RefreshToken> query = this.Context.Set<RefreshToken>();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        var tokens = await query
            .Where(t => t.DeviceId == deviceId)
            .Include(t => t.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(t => t.User)
            .ThenInclude(u => u.RefreshTokens)
            .ToListAsync(cancellationToken);

        return tokens;
    }

    public async Task<IReadOnlyList<UserFavoriteProblem>> GetFavoriteProblemsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var problems = await this.Context.UserFavoriteProblems
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        return problems;
    }

    public async Task<UserFavoriteProblem> FavoriteProblemForUserAsync(int userId, int problemId, CancellationToken cancellationToken = default)
    {
        var favorite = new UserFavoriteProblem
        {
            UserId = userId,
            ProblemId = problemId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            this.Context.UserFavoriteProblems.Add(favorite);
            await this.SaveChangesAsync(cancellationToken);

            return favorite;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg) // postgres specific logic
        {
            // 23505 = unique_violation (includes PK violation)
            if (pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return favorite; // already favorited
            }

            // 23503 = foreign_key_violation (user or problem doesn't exist)
            if (pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                throw new KeyNotFoundException($"User or problem not found. UserId {userId} ProblemId {problemId}", ex);
            }

            throw;
        }
    }

    public async Task<bool> UnfavoriteProblemForUserAsync(int userId, int problemId, CancellationToken cancellationToken = default)
    {
        var rows = await this.Context.UserFavoriteProblems
            .Where(x => x.UserId == userId && x.ProblemId == problemId)
            .ExecuteDeleteAsync(cancellationToken);

        return rows == 1;
    }

    protected override IQueryable<User> AddIncludes(IQueryable<User> query)
    {
        return query
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .Include(user => user.LinkedAccounts);
    }
}