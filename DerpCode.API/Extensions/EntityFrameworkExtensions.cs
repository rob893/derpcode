using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Core;
using DerpCode.API.Models;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DerpCode.API.Extensions;

/// <summary>
/// Extension methods for Entity Framework Core.
/// </summary>
public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Delegate for ordering IQueryable sources.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="isFirst">True if this is for 'first' pagination, false for 'last'.</param>
    /// <returns>The ordered queryable.</returns>
    private delegate IOrderedQueryable<TEntity> QueryOrderer<TEntity>(IQueryable<TEntity> source, bool isFirst) where TEntity : class;

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

        // Apply cursor filtering
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

        // Define ordering logic for simple cursor pagination
        QueryOrderer<TEntity> orderQuery = (source, isFirst) =>
            isFirst ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);

        // Create cursor function
        var keySelectorCompiled = keySelector.Compile();
        Func<TEntity, string> createCursor = entity => keyConverter(keySelectorCompiled(entity));

        return await ExecutePaginationAsync<TEntity, TEntityKey>(
            src,
            orderQuery,
            first,
            last,
            afterCursor,
            beforeCursor,
            includeTotal,
            createCursor,
            cancellationToken);
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
            key => key.ConvertToBase64UrlEncodedString(),
            cursor => cursor.ConvertToInt32FromBase64UrlEncodedString(),
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            cancellationToken);
    }

    /// <summary>
    /// Creates a cursor-paginated list from an IQueryable source with ordering by a specified field.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key used for cursor-based pagination.</typeparam>
    /// <typeparam name="TOrderKey">The type of the field used for ordering.</typeparam>
    /// <param name="src">The IQueryable source.</param>
    /// <param name="keySelector">Expression to select the key from the entity.</param>
    /// <param name="orderSelector">Expression to select the ordering field from the entity.</param>
    /// <param name="compositeKeyConverter">Function to convert the composite key (order value and entity key) to a string cursor.</param>
    /// <param name="compositeCursorConverter">Function to convert a string cursor back to a composite key (order value and entity key).</param>
    /// <param name="first">Number of items to take from the beginning of the result set.</param>
    /// <param name="last">Number of items to take from the end of the result set.</param>
    /// <param name="afterCursor">Cursor indicating to start after this position.</param>
    /// <param name="beforeCursor">Cursor indicating to end before this position.</param>
    /// <param name="includeTotal">Whether to include the total count of items.</param>
    /// <param name="ascending">Whether to order in ascending order. Default is true.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A cursor paginated list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="NotSupportedException">Thrown when both first and last parameters are provided.</exception>
    /// <exception cref="ArgumentException">Thrown when first or last is less than 0.</exception>
    public static async Task<CursorPaginatedList<TEntity, TEntityKey>> ToCursorPaginatedListAsync<TEntity, TEntityKey, TOrderKey>(
        this IQueryable<TEntity> src,
        Expression<Func<TEntity, TEntityKey>> keySelector,
        Expression<Func<TEntity, TOrderKey>> orderSelector,
        Func<(TOrderKey OrderValue, TEntityKey Key), string> compositeKeyConverter,
        Func<string, (TOrderKey OrderValue, TEntityKey Key)> compositeCursorConverter,
        int? first,
        int? last,
        string? afterCursor,
        string? beforeCursor,
        bool includeTotal,
        bool ascending = true,
        CancellationToken cancellationToken = default)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
            where TOrderKey : IComparable<TOrderKey>
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(orderSelector);
        ArgumentNullException.ThrowIfNull(compositeKeyConverter);
        ArgumentNullException.ThrowIfNull(compositeCursorConverter);

        // Apply cursor filtering
        if (afterCursor != null)
        {
            var (afterOrderValue, afterKey) = compositeCursorConverter(afterCursor);

            if (ascending)
            {
                // For ascending order: (orderValue > afterOrderValue) OR (orderValue == afterOrderValue AND key > afterKey)
                src = src.Where(BuildCompositeAfterFilter(orderSelector, keySelector, afterOrderValue, afterKey, true));
            }
            else
            {
                // For descending order: (orderValue < afterOrderValue) OR (orderValue == afterOrderValue AND key > afterKey)
                src = src.Where(BuildCompositeAfterFilter(orderSelector, keySelector, afterOrderValue, afterKey, false));
            }
        }

        if (beforeCursor != null)
        {
            var (beforeOrderValue, beforeKey) = compositeCursorConverter(beforeCursor);

            if (ascending)
            {
                // For ascending order: (orderValue < beforeOrderValue) OR (orderValue == beforeOrderValue AND key < beforeKey)
                src = src.Where(BuildCompositeBeforeFilter(orderSelector, keySelector, beforeOrderValue, beforeKey, true));
            }
            else
            {
                // For descending order: (orderValue > beforeOrderValue) OR (orderValue == beforeOrderValue AND key < beforeKey)
                src = src.Where(BuildCompositeBeforeFilter(orderSelector, keySelector, beforeOrderValue, beforeKey, false));
            }
        }

        // Define ordering logic for composite cursor pagination
        QueryOrderer<TEntity> orderQuery = (source, isFirst) =>
        {
            if (isFirst)
            {
                // For 'first' pagination, order by the specified field, then by key for consistent ordering
                return ascending
                    ? source.OrderBy(orderSelector).ThenBy(keySelector)
                    : source.OrderByDescending(orderSelector).ThenBy(keySelector);
            }
            else
            {
                // For 'last' pagination, reverse the ordering to get the last N items
                return ascending
                    ? source.OrderByDescending(orderSelector).ThenByDescending(keySelector)
                    : source.OrderBy(orderSelector).ThenByDescending(keySelector);
            }
        };

        // Create cursor function
        var keySelectorCompiled = keySelector.Compile();
        var orderSelectorCompiled = orderSelector.Compile();
        Func<TEntity, string> createCursor = entity =>
            compositeKeyConverter((orderSelectorCompiled(entity), keySelectorCompiled(entity)));

        return await ExecutePaginationAsync<TEntity, TEntityKey>(
            src,
            orderQuery,
            first,
            last,
            afterCursor,
            beforeCursor,
            includeTotal,
            createCursor,
            cancellationToken);
    }


    /// <summary>
    /// Creates a cursor-paginated list from an IQueryable source with ordering by a specified field using query parameters.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key used for cursor-based pagination.</typeparam>
    /// <typeparam name="TOrderKey">The type of the field used for ordering.</typeparam>
    /// <param name="src">The IQueryable source.</param>
    /// <param name="keySelector">Expression to select the key from the entity.</param>
    /// <param name="orderSelector">Expression to select the ordering field from the entity.</param>
    /// <param name="compositeKeyConverter">Function to convert the composite key (order value and entity key) to a string cursor.</param>
    /// <param name="compositeCursorConverter">Function to convert a string cursor back to a composite key (order value and entity key).</param>
    /// <param name="queryParameters">The pagination query parameters.</param>
    /// <param name="ascending">Whether to order in ascending order. Default is true.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A cursor paginated list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when queryParameters is null.</exception>
    public static Task<CursorPaginatedList<TEntity, TEntityKey>> ToCursorPaginatedListAsync<TEntity, TEntityKey, TOrderKey>(
        this IQueryable<TEntity> src,
        Expression<Func<TEntity, TEntityKey>> keySelector,
        Expression<Func<TEntity, TOrderKey>> orderSelector,
        Func<(TOrderKey OrderValue, TEntityKey Key), string> compositeKeyConverter,
        Func<string, (TOrderKey OrderValue, TEntityKey Key)> compositeCursorConverter,
        CursorPaginationQueryParameters queryParameters,
        bool ascending = true,
        CancellationToken cancellationToken = default)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
            where TOrderKey : IComparable<TOrderKey>
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        return src.ToCursorPaginatedListAsync(
            keySelector,
            orderSelector,
            compositeKeyConverter,
            compositeCursorConverter,
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending,
            cancellationToken);
    }

    /// <summary>
    /// Creates a cursor-paginated list from an IQueryable source of entities with integer IDs, ordered by a specified field.
    /// This method creates composite cursors containing both the order field value and the entity ID.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that implements IIdentifiable with int key.</typeparam>
    /// <typeparam name="TOrderKey">The type of the field used for ordering.</typeparam>
    /// <param name="src">The IQueryable source.</param>
    /// <param name="orderSelector">Expression to select the ordering field from the entity.</param>
    /// <param name="queryParameters">The pagination query parameters.</param>
    /// <param name="ascending">Whether to order in ascending order. Default is true.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A cursor paginated list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when queryParameters is null.</exception>
    public static Task<CursorPaginatedList<TEntity, int>> ToCursorPaginatedListAsync<TEntity, TOrderKey>(
        this IQueryable<TEntity> src,
        Expression<Func<TEntity, TOrderKey>> orderSelector,
        CursorPaginationQueryParameters queryParameters,
        bool ascending = true,
        CancellationToken cancellationToken = default)
            where TEntity : class, IIdentifiable<int>
            where TOrderKey : IComparable<TOrderKey>
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        return src.ToCursorPaginatedListAsync(
            item => item.Id,
            orderSelector,
            CursorConverters.CreateCompositeKeyConverter<TOrderKey, int>(),
            CursorConverters.CreateCompositeCursorConverter<TOrderKey, int>(),
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending,
            cancellationToken);
    }

    /// <summary>
    /// Common helper method for executing cursor pagination logic.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key used for cursor-based pagination.</typeparam>
    /// <param name="src">The filtered IQueryable source.</param>
    /// <param name="orderQuery">Function to order the query based on pagination direction.</param>
    /// <param name="first">Number of items to take from the beginning of the result set.</param>
    /// <param name="last">Number of items to take from the end of the result set.</param>
    /// <param name="afterCursor">Cursor indicating to start after this position.</param>
    /// <param name="beforeCursor">Cursor indicating to end before this position.</param>
    /// <param name="includeTotal">Whether to include the total count of items.</param>
    /// <param name="createCursor">Function to create cursor from entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A cursor paginated list.</returns>
    private static async Task<CursorPaginatedList<TEntity, TEntityKey>> ExecutePaginationAsync<TEntity, TEntityKey>(
        IQueryable<TEntity> src,
        QueryOrderer<TEntity> orderQuery,
        int? first,
        int? last,
        string? afterCursor,
        string? beforeCursor,
        bool includeTotal,
        Func<TEntity, string> createCursor,
        CancellationToken cancellationToken)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        if (first != null && last != null)
        {
            throw new NotSupportedException($"Passing both `{nameof(first)}` and `{nameof(last)}` to paginate is not supported.");
        }

        List<TEntity> pageList;
        var hasNextPage = beforeCursor != null;
        var hasPreviousPage = afterCursor != null;

        if (first != null)
        {
            if (first.Value < 0)
            {
                throw new ArgumentException($"{nameof(first)} cannot be less than 0.", nameof(first));
            }

            var orderedQuery = orderQuery(src, isFirst: true);
            pageList = await orderedQuery.Take(first.Value + 1).ToListAsync(cancellationToken);

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

            var orderedQuery = orderQuery(src, isFirst: false);
            pageList = await orderedQuery.Take(last.Value + 1).ToListAsync(cancellationToken);

            hasPreviousPage = pageList.Count > last.Value;

            if (hasPreviousPage)
            {
                pageList.RemoveAt(pageList.Count - 1);
            }

            pageList.Reverse();
        }
        else
        {
            var orderedQuery = orderQuery(src, isFirst: true);
            pageList = await orderedQuery.ToListAsync(cancellationToken);
        }

        var firstPageItem = pageList.FirstOrDefault();
        var lastPageItem = pageList.LastOrDefault();

        return new CursorPaginatedList<TEntity, TEntityKey>(
            pageList,
            hasNextPage,
            hasPreviousPage,
            firstPageItem != null ? createCursor(firstPageItem) : null,
            lastPageItem != null ? createCursor(lastPageItem) : null,
            includeTotal ? await src.CountAsync(cancellationToken) : null);
    }

    /// <summary>
    /// Builds a composite filter expression for "after" cursor filtering.
    /// </summary>
    private static Expression<Func<TEntity, bool>> BuildCompositeAfterFilter<TEntity, TEntityKey, TOrderKey>(
        Expression<Func<TEntity, TOrderKey>> orderSelector,
        Expression<Func<TEntity, TEntityKey>> keySelector,
        TOrderKey afterOrderValue,
        TEntityKey afterKey,
        bool ascending)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
            where TOrderKey : IComparable<TOrderKey>
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");

        // Replace parameters in selectors with our unified parameter
        var orderBody = new ParameterReplacer(orderSelector.Parameters[0], parameter).Visit(orderSelector.Body);
        var keyBody = new ParameterReplacer(keySelector.Parameters[0], parameter).Visit(keySelector.Body);

        var orderConstant = Expression.Constant(afterOrderValue);
        var keyConstant = Expression.Constant(afterKey);

        Expression condition;
        if (ascending)
        {
            // (orderValue > afterOrderValue) OR (orderValue == afterOrderValue AND key > afterKey)
            var orderGreater = Expression.GreaterThan(orderBody, orderConstant);
            var orderEqual = Expression.Equal(orderBody, orderConstant);
            var keyGreater = Expression.GreaterThan(keyBody, keyConstant);
            var equalAndKeyGreater = Expression.AndAlso(orderEqual, keyGreater);
            condition = Expression.OrElse(orderGreater, equalAndKeyGreater);
        }
        else
        {
            // (orderValue < afterOrderValue) OR (orderValue == afterOrderValue AND key > afterKey)
            var orderLess = Expression.LessThan(orderBody, orderConstant);
            var orderEqual = Expression.Equal(orderBody, orderConstant);
            var keyGreater = Expression.GreaterThan(keyBody, keyConstant);
            var equalAndKeyGreater = Expression.AndAlso(orderEqual, keyGreater);
            condition = Expression.OrElse(orderLess, equalAndKeyGreater);
        }

        return Expression.Lambda<Func<TEntity, bool>>(condition, parameter);
    }

    /// <summary>
    /// Builds a composite filter expression for "before" cursor filtering.
    /// </summary>
    private static Expression<Func<TEntity, bool>> BuildCompositeBeforeFilter<TEntity, TEntityKey, TOrderKey>(
        Expression<Func<TEntity, TOrderKey>> orderSelector,
        Expression<Func<TEntity, TEntityKey>> keySelector,
        TOrderKey beforeOrderValue,
        TEntityKey beforeKey,
        bool ascending)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
            where TOrderKey : IComparable<TOrderKey>
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entity");

        // Replace parameters in selectors with our unified parameter
        var orderBody = new ParameterReplacer(orderSelector.Parameters[0], parameter).Visit(orderSelector.Body);
        var keyBody = new ParameterReplacer(keySelector.Parameters[0], parameter).Visit(keySelector.Body);

        var orderConstant = Expression.Constant(beforeOrderValue);
        var keyConstant = Expression.Constant(beforeKey);

        Expression condition;
        if (ascending)
        {
            // (orderValue < beforeOrderValue) OR (orderValue == beforeOrderValue AND key < beforeKey)
            var orderLess = Expression.LessThan(orderBody, orderConstant);
            var orderEqual = Expression.Equal(orderBody, orderConstant);
            var keyLess = Expression.LessThan(keyBody, keyConstant);
            var equalAndKeyLess = Expression.AndAlso(orderEqual, keyLess);
            condition = Expression.OrElse(orderLess, equalAndKeyLess);
        }
        else
        {
            // (orderValue > beforeOrderValue) OR (orderValue == beforeOrderValue AND key < beforeKey)
            var orderGreater = Expression.GreaterThan(orderBody, orderConstant);
            var orderEqual = Expression.Equal(orderBody, orderConstant);
            var keyLess = Expression.LessThan(keyBody, keyConstant);
            var equalAndKeyLess = Expression.AndAlso(orderEqual, keyLess);
            condition = Expression.OrElse(orderGreater, equalAndKeyLess);
        }

        return Expression.Lambda<Func<TEntity, bool>>(condition, parameter);
    }

    /// <summary>
    /// Helper class to replace parameters in expression trees.
    /// </summary>
    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression oldParameter;

        private readonly ParameterExpression newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            this.oldParameter = oldParameter ?? throw new ArgumentNullException(nameof(oldParameter));
            this.newParameter = newParameter ?? throw new ArgumentNullException(nameof(newParameter));
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == this.oldParameter ? this.newParameter : base.VisitParameter(node);
        }
    }
}