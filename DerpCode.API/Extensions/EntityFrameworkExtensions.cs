using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models;
using DerpCode.API.Models.QueryParameters;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Extensions;

/// <summary>
/// Extension methods for Entity Framework Core.
/// </summary>
public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Removes all entities from the specified DbSet.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dbSet">The DbSet to clear.</param>
    /// <exception cref="ArgumentNullException">Thrown when dbSet is null.</exception>
    public static void Clear<T>(this DbSet<T> dbSet) where T : class
    {
        ArgumentNullException.ThrowIfNull(dbSet);

        dbSet.RemoveRange(dbSet);
    }

    /// <summary>
    /// Creates a cursor-paginated list from an IQueryable source.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key used for cursor-based pagination.</typeparam>
    /// <param name="src">The IQueryable source.</param>
    /// <param name="keySelector">Expression to select the key from the entity.</param>
    /// <param name="keyConverter">Function to convert the key to a string cursor.</param>
    /// <param name="cursorConverter">Function to convert a string cursor back to a key.</param>
    /// <param name="first">Number of items to take from the beginning of the result set.</param>
    /// <param name="last">Number of items to take from the end of the result set.</param>
    /// <param name="afterCursor">Cursor indicating to start after this position.</param>
    /// <param name="beforeCursor">Cursor indicating to end before this position.</param>
    /// <param name="includeTotal">Whether to include the total count of items.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A cursor paginated list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when src, keySelector, keyConverter, or cursorConverter is null.</exception>
    /// <exception cref="NotSupportedException">Thrown when both first and last parameters are provided.</exception>
    /// <exception cref="ArgumentException">Thrown when first or last is less than 0.</exception>
    public static async Task<CursorPaginatedList<TEntity, TEntityKey>> ToCursorPaginatedListAsync<TEntity, TEntityKey>(
        this IQueryable<TEntity> src,
        Expression<Func<TEntity, TEntityKey>> keySelector,
        Func<TEntityKey, string> keyConverter,
        Func<string, TEntityKey> cursorConverter,
        int? first,
        int? last,
        string? afterCursor,
        string? beforeCursor,
        bool includeTotal,
        CancellationToken cancellationToken = default)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(keyConverter);
        ArgumentNullException.ThrowIfNull(cursorConverter);

        if (first != null && last != null)
        {
            throw new NotSupportedException($"Passing both `{nameof(first)}` and `{nameof(last)}` to paginate is not supported.");
        }

        if (afterCursor != null)
        {
            var after = cursorConverter(afterCursor);
            src = src.Where(keySelector.Apply(key => key.CompareTo(after) > 0));
        }

        if (beforeCursor != null)
        {
            var before = cursorConverter(beforeCursor);
            src = src.Where(keySelector.Apply(key => key.CompareTo(before) < 0));
        }

        var pageList = new List<TEntity>();
        var hasNextPage = beforeCursor != null;
        var hasPreviousPage = afterCursor != null;

        if (first != null)
        {
            if (first.Value < 0)
            {
                throw new ArgumentException($"{nameof(first)} cannot be less than 0.", nameof(first));
            }

            pageList = await src.OrderBy(keySelector).Take(first.Value + 1).ToListAsync(cancellationToken);

            hasNextPage = pageList.Count > first.Value;

            if (hasNextPage)
            {
                pageList.RemoveAt(pageList.Count - 1);
            }
        }
        else if (last != null)
        {
            if (last.Value < 0)
            {
                throw new ArgumentException($"{nameof(last)} cannot be less than 0.", nameof(last));
            }

            pageList = await src.OrderByDescending(keySelector).Take(last.Value + 1).ToListAsync(cancellationToken);

            hasPreviousPage = pageList.Count > last.Value;

            if (hasPreviousPage)
            {
                pageList.RemoveAt(pageList.Count - 1);
            }

            pageList.Reverse();
        }
        else
        {
            pageList = await src.OrderBy(keySelector).ToListAsync(cancellationToken);
        }

        var firstPageItem = pageList.FirstOrDefault();
        var lastPageItem = pageList.LastOrDefault();

        var keySelectorCompiled = keySelector.Compile();

        return new CursorPaginatedList<TEntity, TEntityKey>(
            pageList,
            hasNextPage,
            hasPreviousPage,
            firstPageItem != null ? keyConverter(keySelectorCompiled(firstPageItem)) : null,
            lastPageItem != null ? keyConverter(keySelectorCompiled(lastPageItem)) : null,
            includeTotal ? await src.CountAsync(cancellationToken) : null);
    }

    /// <summary>
    /// Creates a cursor-paginated list from an IQueryable source using query parameters.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key used for cursor-based pagination.</typeparam>
    /// <param name="src">The IQueryable source.</param>
    /// <param name="keySelector">Expression to select the key from the entity.</param>
    /// <param name="keyConverter">Function to convert the key to a string cursor.</param>
    /// <param name="cursorConverter">Function to convert a string cursor back to a key.</param>
    /// <param name="queryParameters">The pagination query parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A cursor paginated list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when queryParameters is null.</exception>
    public static Task<CursorPaginatedList<TEntity, TEntityKey>> ToCursorPaginatedListAsync<TEntity, TEntityKey>(
        this IQueryable<TEntity> src,
        Expression<Func<TEntity, TEntityKey>> keySelector,
        Func<TEntityKey, string> keyConverter,
        Func<string, TEntityKey> cursorConverter,
        CursorPaginationQueryParameters queryParameters,
        CancellationToken cancellationToken = default)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        return src.ToCursorPaginatedListAsync(
            keySelector,
            keyConverter,
            cursorConverter,
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            cancellationToken);
    }

    /// <summary>
    /// Creates a cursor-paginated list from an IQueryable source of entities with integer IDs.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that implements IIdentifiable with int key.</typeparam>
    /// <param name="src">The IQueryable source.</param>
    /// <param name="queryParameters">The pagination query parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A cursor paginated list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when queryParameters is null.</exception>
    public static Task<CursorPaginatedList<TEntity, int>> ToCursorPaginatedListAsync<TEntity>(
        this IQueryable<TEntity> src,
        CursorPaginationQueryParameters queryParameters,
        CancellationToken cancellationToken = default)
            where TEntity : class, IIdentifiable<int>
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        return src.ToCursorPaginatedListAsync(
            item => item.Id,
            key => key.ConvertToBase64Url(),
            cursor => cursor.ConvertToInt32FromBase64Url(),
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            cancellationToken);
    }
}