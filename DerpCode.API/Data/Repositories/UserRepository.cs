using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Data.Repositories;

public sealed class UserRepository : Repository<User, CursorPaginationQueryParameters>, IUserRepository
{
    public UserManager<User> UserManager { get; init; }

    private readonly SignInManager<User> signInManager;

    public UserRepository(DataContext context, UserManager<User> userManager, SignInManager<User> signInManager) : base(context)
    {
        this.UserManager = userManager;
        this.signInManager = signInManager;
    }

    public async Task<IdentityResult> CreateUserWithAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Created = DateTime.UtcNow;
        var created = await this.UserManager.CreateAsync(user);
        await this.UserManager.AddToRoleAsync(user, "User");

        return created;
    }

    public async Task<IdentityResult> CreateUserWithPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.Created = DateTime.UtcNow;
        var created = await this.UserManager.CreateAsync(user, password);
        await this.UserManager.AddToRoleAsync(user, "User");

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
        IQueryable<User> query = this.Context.Users;

        query = this.AddIncludes(query);
        query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

        return query.OrderBy(e => e.Id).FirstOrDefaultAsync(user => user.UserName == username, cancellationToken);
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

    public Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return this.Context.Roles.ToListAsync(cancellationToken);
    }

    protected override IQueryable<User> AddIncludes(IQueryable<User> query)
    {
        return query
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .Include(user => user.LinkedAccounts);
    }
}