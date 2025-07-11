using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Extensions;
using DerpCode.API.Models;
using DerpCode.API.Models.QueryParameters;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Data.Repositories;

public abstract class Repository<TEntity, TEntityKey, TSearchParams> : IRepository<TEntity, TEntityKey, TSearchParams>
    where TEntity : class, IIdentifiable<TEntityKey>
    where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    where TSearchParams : CursorPaginationQueryParameters
{
    protected Repository(DataContext context, Func<TEntityKey, string> convertIdToBase64, Func<string, TEntityKey> convertBase64ToIdType)
    {
        this.Context = context ?? throw new ArgumentNullException(nameof(context));
        this.ConvertIdToBase64 = convertIdToBase64 ?? throw new ArgumentNullException(nameof(convertIdToBase64));
        this.ConvertBase64ToIdType = convertBase64ToIdType ?? throw new ArgumentNullException(nameof(convertBase64ToIdType));
    }

    protected DataContext Context { get; }

    protected Func<TEntityKey, string> ConvertIdToBase64 { get; }

    protected Func<string, TEntityKey> ConvertBase64ToIdType { get; }

    public void Add(TEntity entity)
    {
        this.Context.Set<TEntity>().Add(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        this.Context.Set<TEntity>().AddRange(entities);
    }

    public void Remove(TEntity entity)
    {
        this.BeforeRemove(entity);
        this.Context.Set<TEntity>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
        {
            this.BeforeRemove(entity);
        }

        this.Context.Set<TEntity>().RemoveRange(entities);
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return this.Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition, bool track = true, CancellationToken cancellationToken = default)
    {
        return await this.FirstOrDefaultAsync(condition, [], track, cancellationToken);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, object>>[] includes, bool track = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = this.Context.Set<TEntity>();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        query = this.AddIncludes(query);
        query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

        var item = await query.OrderBy(e => e.Id).FirstOrDefaultAsync(condition, cancellationToken);

        if (item != null)
        {
            this.PostProcess(item);
        }

        return item;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TEntityKey id, bool track = true, CancellationToken cancellationToken = default)
    {
        return await this.GetByIdAsync(id, [], track, cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(TEntityKey id, Expression<Func<TEntity, object>>[] includes, bool track = true, CancellationToken cancellationToken = default)
    {
        return await this.FirstOrDefaultAsync(
            entity => entity.Id.Equals(id),
            includes,
            track,
            cancellationToken);
    }

    public virtual async Task<List<TEntity>> SearchAsync(Expression<Func<TEntity, bool>> condition, bool track = true, CancellationToken cancellationToken = default)
    {
        return await this.SearchAsync(condition, [], track, cancellationToken);
    }

    public virtual async Task<List<TEntity>> SearchAsync(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, object>>[] includes, bool track = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = this.Context.Set<TEntity>();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        query = this.AddIncludes(query);
        query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

        var list = await query.Where(condition).ToListAsync(cancellationToken);

        foreach (var item in list)
        {
            this.PostProcess(item);
        }

        return list;
    }

    public virtual async Task<CursorPaginatedList<TEntity, TEntityKey>> SearchAsync(TSearchParams searchParams, bool track = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = this.Context.Set<TEntity>();

        if (!track)
        {
            query = query.AsNoTracking();
        }

        query = this.AddIncludes(query);
        query = this.AddWhereClauses(query, searchParams);

        var list = await query.ToCursorPaginatedListAsync(
            item => item.Id,
            this.ConvertIdToBase64,
            this.ConvertBase64ToIdType,
            searchParams,
            cancellationToken);

        foreach (var item in list)
        {
            this.PostProcess(item);
        }

        return list;
    }

    protected virtual IQueryable<TEntity> AddWhereClauses(IQueryable<TEntity> query, TSearchParams searchParams)
    {
        return query;
    }

    protected virtual void BeforeRemove(TEntity entity) { }

    protected virtual void PostProcess(TEntity entity) { }

    protected virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query)
    {
        return query;
    }
}

public abstract class Repository<TEntity, TSearchParams> : Repository<TEntity, int, TSearchParams>, IRepository<TEntity, int, TSearchParams>
    where TEntity : class, IIdentifiable<int>
    where TSearchParams : CursorPaginationQueryParameters
{
    protected Repository(DataContext context) : base(
        context,
        Id => Id.ConvertToBase64Url(),
        str =>
        {
            try
            {
                return str.ConvertToInt32FromBase64Url();
            }
            catch
            {
                throw new ArgumentException($"{str} is not a valid base 64 encoded int32.");
            }
        })
    { }
}