using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public interface IRepository<TEntity, TEntityKey, TSearchParams>
    where TEntity : class, IIdentifiable<TEntityKey>
    where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    where TSearchParams : CursorPaginationQueryParameters
{
    void Add(TEntity entity);

    void AddRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition, bool track = true, CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, object>>[] includes, bool track = true, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(TEntityKey id, bool track = true, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(TEntityKey id, Expression<Func<TEntity, object>>[] includes, bool track = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> SearchAsync(Expression<Func<TEntity, bool>> condition, bool track = true, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> SearchAsync(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, object>>[] includes, bool track = true, CancellationToken cancellationToken = default);

    Task<CursorPaginatedList<TEntity, TEntityKey>> SearchAsync(TSearchParams searchParams, bool track = true, CancellationToken cancellationToken = default);
}

public interface IRepository<TEntity, TSearchParams> : IRepository<TEntity, int, TSearchParams>
    where TEntity : class, IIdentifiable<int>
    where TSearchParams : CursorPaginationQueryParameters
{ }