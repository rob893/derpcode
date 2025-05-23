using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using Microsoft.AspNetCore.Identity;

namespace DerpCode.API.Data.Repositories;

public interface IUserRepository : IRepository<User, CursorPaginationQueryParameters>
{
    UserManager<User> UserManager { get; }

    Task<IdentityResult> CreateUserWithoutPasswordAsync(User user, CancellationToken cancellationToken = default);

    Task<IdentityResult> CreateUserWithPasswordAsync(User user, string password, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetByLinkedAccountAsync(string id, LinkedAccountType accountType, Expression<Func<User, object>>[] includes, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(string username, Expression<Func<User, object>>[] includes, CancellationToken cancellationToken = default);

    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);

    Task<CursorPaginatedList<Role, int>> GetRolesAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken = default);

    Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken = default);
}
