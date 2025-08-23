using System;
using System.Collections.Generic;
using System.Linq;
using DerpCode.API.Core;
using DerpCode.API.Models;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Utilities;

namespace DerpCode.API.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Delegate for ordering IEnumerable sources.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <param name="isFirst">True if this is for 'first' pagination, false for 'last'.</param>
    /// <returns>The ordered enumerable.</returns>
    private delegate IOrderedEnumerable<TEntity> EnumerableOrderer<TEntity>(IEnumerable<TEntity> source, bool isFirst) where TEntity : class;

    /// <summary>
    /// Converts an IEnumerable to a CursorPaginatedResponse.
    /// </summary>
    /// <param name="src">The collection.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="keyConverter">The key converter.</param>
    /// <param name="cursorConverter">The cursor converter.</param>
    /// <param name="queryParameters">The query parameters.</param>
    /// <typeparam name="TEntity">Type of entity.</typeparam>
    /// <typeparam name="TEntityKey">Type of entity key.</typeparam>
    /// <returns>The CursorPaginatedResponse.</returns>
    public static CursorPaginatedResponse<TEntity, TEntityKey> ToCursorPaginatedResponse<TEntity, TEntityKey>(
        this IEnumerable<TEntity> src,
        Func<TEntity, TEntityKey> keySelector,
        Func<TEntityKey, string> keyConverter,
        Func<string, TEntityKey> cursorConverter,
        CursorPaginationQueryParameters queryParameters)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(keyConverter);
        ArgumentNullException.ThrowIfNull(cursorConverter);
        ArgumentNullException.ThrowIfNull(queryParameters);

        if (queryParameters.First != null && queryParameters.Last != null)
        {
            throw new NotSupportedException($"Passing both `{nameof(queryParameters.First)}` and `{nameof(queryParameters.Last)}` to paginate is not supported.");
        }

        if (src is CursorPaginatedList<TEntity, TEntityKey> cursorList)
        {
            return new CursorPaginatedResponse<TEntity, TEntityKey>
            {
                Edges = queryParameters.IncludeEdges ? cursorList.Select(
                    item => new CursorPaginatedResponseEdge<TEntity>
                    {
                        Cursor = keyConverter(keySelector(item)),
                        Node = item
                    }) : null,
                Nodes = queryParameters.IncludeNodes ? cursorList : null,
                PageInfo = new CursorPaginatedResponsePageInfo
                {
                    StartCursor = cursorList.StartCursor,
                    EndCursor = cursorList.EndCursor,
                    HasNextPage = cursorList.HasNextPage,
                    HasPreviousPage = cursorList.HasPreviousPage,
                    PageCount = cursorList.PageCount,
                    TotalCount = cursorList.TotalCount
                }
            };
        }

        // Materialize the source collection to avoid multiple enumerations
        var srcList = src.ToList();
        int? totalCount = queryParameters.IncludeTotal ? srcList.Count : null;

        // Start from the ordered source list
        var orderedItems = srcList.OrderBy(keySelector).AsEnumerable();

        if (queryParameters.After != null)
        {
            var after = cursorConverter(queryParameters.After);
            orderedItems = orderedItems.Where(item => keySelector(item).CompareTo(after) > 0);
        }

        if (queryParameters.Before != null)
        {
            var before = cursorConverter(queryParameters.Before);
            orderedItems = orderedItems.Where(item => keySelector(item).CompareTo(before) < 0);
        }

        if (queryParameters.First != null)
        {
            if (queryParameters.First.Value < 0)
            {
                throw new ArgumentException($"{nameof(queryParameters.First)} cannot be less than 0.", nameof(queryParameters));
            }

            orderedItems = orderedItems.Take(queryParameters.First.Value);
        }
        else if (queryParameters.Last != null)
        {
            if (queryParameters.Last.Value < 0)
            {
                throw new ArgumentException($"{nameof(queryParameters.Last)} cannot be less than 0.", nameof(queryParameters));
            }

            orderedItems = orderedItems.TakeLast(queryParameters.Last.Value);
        }

        // Materialize filtered and ordered items to avoid multiple enumerations
        var orderedItemsList = orderedItems.ToList();

        // Create edges once (instead of potentially multiple times when accessing)
        var pageList = orderedItemsList.Select(item => new CursorPaginatedResponseEdge<TEntity>
        {
            Cursor = keyConverter(keySelector(item)),
            Node = item
        }).ToList();

        var firstPageItem = pageList.FirstOrDefault();
        var lastPageItem = pageList.LastOrDefault();

        var firstSrcItem = srcList.FirstOrDefault();
        var lastSrcItem = srcList.LastOrDefault();

        return new CursorPaginatedResponse<TEntity, TEntityKey>
        {
            Edges = queryParameters.IncludeEdges ? pageList : null,
            Nodes = queryParameters.IncludeNodes ? orderedItemsList : null,
            PageInfo = new CursorPaginatedResponsePageInfo
            {
                StartCursor = firstPageItem?.Cursor,
                EndCursor = lastPageItem?.Cursor,
                HasNextPage = lastPageItem != null && lastSrcItem != null && keySelector(lastSrcItem).CompareTo(keySelector(lastPageItem.Node)) > 0,
                HasPreviousPage = firstPageItem != null && firstSrcItem != null && keySelector(firstSrcItem).CompareTo(keySelector(firstPageItem.Node)) < 0,
                PageCount = pageList.Count,
                TotalCount = totalCount
            }
        };
    }

    /// <summary>
    /// Converts an IEnumerable to a CursorPaginatedResponse.
    /// </summary>
    /// <param name="src">The collection.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="queryParameters">The query parameters.</param>
    /// <typeparam name="TEntity">Type of entity.</typeparam>
    /// <returns>The CursorPaginatedResponse.</returns>
    public static CursorPaginatedResponse<TEntity, int> ToCursorPaginatedResponse<TEntity>(
        this IEnumerable<TEntity> src,
        Func<TEntity, int> keySelector,
        CursorPaginationQueryParameters queryParameters)
            where TEntity : class
    {
        return src.ToCursorPaginatedResponse(
            keySelector,
            key => key.ConvertToBase64Url(),
            cursor => cursor.ConvertToInt32FromBase64Url(),
            queryParameters);
    }

    /// <summary>
    /// Converts an IEnumerable to a CursorPaginatedResponse.
    /// </summary>
    /// <param name="src">The collection.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="queryParameters">The query parameters.</param>
    /// <typeparam name="TEntity">Type of entity.</typeparam>
    /// <returns>The CursorPaginatedResponse.</returns>
    public static CursorPaginatedResponse<TEntity, string> ToCursorPaginatedResponse<TEntity>(
        this IEnumerable<TEntity> src,
        Func<TEntity, string> keySelector,
        CursorPaginationQueryParameters queryParameters)
            where TEntity : class
    {
        return src.ToCursorPaginatedResponse(
            keySelector,
            key => key.ConvertToBase64Url(),
            cursor => cursor.ConvertToStringFromBase64Url(),
            queryParameters);
    }

    /// <summary>
    /// Converts an IEnumerable to a CursorPaginatedResponse.
    /// </summary>
    /// <param name="src">The collection.</param>
    /// <param name="keySelector">The key selector.</param>
    /// <param name="queryParameters">The query parameters.</param>
    /// <typeparam name="TEntity">Type of entity.</typeparam>
    /// <returns>The CursorPaginatedResponse.</returns>
    public static CursorPaginatedResponse<TEntity, long> ToCursorPaginatedResponse<TEntity>(
        this IEnumerable<TEntity> src,
        Func<TEntity, long> keySelector,
        CursorPaginationQueryParameters queryParameters)
            where TEntity : class
    {
        return src.ToCursorPaginatedResponse(
            keySelector,
            key => key.ConvertToBase64Url(),
            cursor => cursor.ConvertToLongFromBase64Url(),
            queryParameters);
    }

    /// <summary>
    /// Converts an IEnumerable to a CursorPaginatedResponse.
    /// </summary>
    /// <param name="src">The collection.</param>
    /// <param name="queryParameters">The query parameters.</param>
    /// <typeparam name="TEntity">Type of entity.</typeparam>
    /// <returns>The CursorPaginatedResponse.</returns>
    public static CursorPaginatedResponse<TEntity, int> ToCursorPaginatedResponse<TEntity>(
        this IEnumerable<TEntity> src,
        CursorPaginationQueryParameters queryParameters)
            where TEntity : class, IIdentifiable<int>
    {
        return src.ToCursorPaginatedResponse(
            item => item.Id,
            key => key.ConvertToBase64Url(),
            cursor => cursor.ConvertToInt32FromBase64Url(),
            queryParameters);
    }

    public static CursorPaginatedList<TEntity, TEntityKey> ToCursorPaginatedList<TEntity, TEntityKey>(
            this IEnumerable<TEntity> src,
            Func<TEntity, TEntityKey> keySelector,
            Func<TEntityKey, string> keyConverter,
            Func<string, TEntityKey> cursorConverter,
            int? first,
            int? last,
            string? afterCursor,
            string? beforeCursor,
            bool includeTotal)
                where TEntity : class
                where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(keyConverter);
        ArgumentNullException.ThrowIfNull(cursorConverter);

        var source = includeTotal ? src.ToList() : src;

        // Apply cursor filtering
        if (afterCursor != null)
        {
            var after = cursorConverter(afterCursor);
            source = source.Where(item => keySelector(item).CompareTo(after) > 0);
        }

        if (beforeCursor != null)
        {
            var before = cursorConverter(beforeCursor);
            source = source.Where(item => keySelector(item).CompareTo(before) < 0);
        }

        // Define ordering logic for simple cursor pagination
        EnumerableOrderer<TEntity> orderSource = (src, isFirst) =>
            isFirst ? src.OrderBy(keySelector) : src.OrderByDescending(keySelector);

        // Create cursor function
        Func<TEntity, string> createCursor = entity => keyConverter(keySelector(entity));

        return ExecutePagination<TEntity, TEntityKey>(
            source,
            orderSource,
            first,
            last,
            afterCursor,
            beforeCursor,
            includeTotal,
            createCursor);
    }

    public static CursorPaginatedList<TEntity, TEntityKey> ToCursorPaginatedList<TEntity, TEntityKey>(
        this IEnumerable<TEntity> src,
        Func<TEntity, TEntityKey> keySelector,
        Func<TEntityKey, string> keyConverter,
        Func<string, TEntityKey> cursorConverter,
        CursorPaginationQueryParameters queryParameters)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        return src.ToCursorPaginatedList(
                keySelector,
                keyConverter,
                cursorConverter,
                queryParameters.First,
                queryParameters.Last,
                queryParameters.After,
                queryParameters.Before,
                queryParameters.IncludeTotal);
    }

    public static CursorPaginatedList<TEntity, int> ToCursorPaginatedList<TEntity>(
        this IEnumerable<TEntity> src,
        CursorPaginationQueryParameters queryParameters)
            where TEntity : class, IIdentifiable<int>
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        return src.ToCursorPaginatedList(
                item => item.Id,
                key => key.ConvertToBase64Url(),
                cursor => cursor.ConvertToInt32FromBase64Url(),
                queryParameters.First,
                queryParameters.Last,
                queryParameters.After,
                queryParameters.Before,
                queryParameters.IncludeTotal);
    }

    /// <summary>
    /// Creates a cursor-paginated list from an IEnumerable source with ordering by a specified field.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key used for cursor-based pagination.</typeparam>
    /// <typeparam name="TOrderKey">The type of the field used for ordering.</typeparam>
    /// <param name="src">The IEnumerable source.</param>
    /// <param name="keySelector">Function to select the key from the entity.</param>
    /// <param name="orderSelector">Function to select the ordering field from the entity.</param>
    /// <param name="compositeKeyConverter">Function to convert the composite key (order value and entity key) to a string cursor.</param>
    /// <param name="compositeCursorConverter">Function to convert a string cursor back to a composite key (order value and entity key).</param>
    /// <param name="first">Number of items to take from the beginning of the result set.</param>
    /// <param name="last">Number of items to take from the end of the result set.</param>
    /// <param name="afterCursor">Cursor indicating to start after this position.</param>
    /// <param name="beforeCursor">Cursor indicating to end before this position.</param>
    /// <param name="includeTotal">Whether to include the total count of items.</param>
    /// <param name="ascending">Whether to order in ascending order. Default is true.</param>
    /// <returns>A cursor paginated list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="NotSupportedException">Thrown when both first and last parameters are provided.</exception>
    /// <exception cref="ArgumentException">Thrown when first or last is less than 0.</exception>
    public static CursorPaginatedList<TEntity, TEntityKey> ToCursorPaginatedList<TEntity, TEntityKey, TOrderKey>(
        this IEnumerable<TEntity> src,
        Func<TEntity, TEntityKey> keySelector,
        Func<TEntity, TOrderKey> orderSelector,
        Func<(TOrderKey OrderValue, TEntityKey Key), string> compositeKeyConverter,
        Func<string, (TOrderKey OrderValue, TEntityKey Key)> compositeCursorConverter,
        int? first,
        int? last,
        string? afterCursor,
        string? beforeCursor,
        bool includeTotal,
        bool ascending = true)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
            where TOrderKey : IComparable<TOrderKey>
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(orderSelector);
        ArgumentNullException.ThrowIfNull(compositeKeyConverter);
        ArgumentNullException.ThrowIfNull(compositeCursorConverter);

        var source = includeTotal ? src.ToList() : src;

        if (first != null && last != null)
        {
            throw new NotSupportedException($"Passing both `{nameof(first)}` and `{nameof(last)}` to paginate is not supported.");
        }

        // Apply cursor filtering
        if (afterCursor != null)
        {
            var (afterOrderValue, afterKey) = compositeCursorConverter(afterCursor);

            if (ascending)
            {
                // For ascending order: (orderValue > afterOrderValue) OR (orderValue == afterOrderValue AND key > afterKey)
                source = source.Where(entity =>
                    orderSelector(entity).CompareTo(afterOrderValue) > 0 ||
                    (orderSelector(entity).CompareTo(afterOrderValue) == 0 && keySelector(entity).CompareTo(afterKey) > 0));
            }
            else
            {
                // For descending order: (orderValue < afterOrderValue) OR (orderValue == afterOrderValue AND key > afterKey)
                source = source.Where(entity =>
                    orderSelector(entity).CompareTo(afterOrderValue) < 0 ||
                    (orderSelector(entity).CompareTo(afterOrderValue) == 0 && keySelector(entity).CompareTo(afterKey) > 0));
            }
        }

        if (beforeCursor != null)
        {
            var (beforeOrderValue, beforeKey) = compositeCursorConverter(beforeCursor);

            if (ascending)
            {
                // For ascending order: (orderValue < beforeOrderValue) OR (orderValue == beforeOrderValue AND key < beforeKey)
                source = source.Where(entity =>
                    orderSelector(entity).CompareTo(beforeOrderValue) < 0 ||
                    (orderSelector(entity).CompareTo(beforeOrderValue) == 0 && keySelector(entity).CompareTo(beforeKey) < 0));
            }
            else
            {
                // For descending order: (orderValue > beforeOrderValue) OR (orderValue == beforeOrderValue AND key < beforeKey)
                source = source.Where(entity =>
                    orderSelector(entity).CompareTo(beforeOrderValue) > 0 ||
                    (orderSelector(entity).CompareTo(beforeOrderValue) == 0 && keySelector(entity).CompareTo(beforeKey) < 0));
            }
        }

        // Define ordering logic for composite cursor pagination
        EnumerableOrderer<TEntity> orderSource = (src, isFirst) =>
        {
            if (isFirst)
            {
                // For 'first' pagination, order by the specified field, then by key for consistent ordering
                return ascending
                    ? src.OrderBy(orderSelector).ThenBy(keySelector)
                    : src.OrderByDescending(orderSelector).ThenBy(keySelector);
            }
            else
            {
                // For 'last' pagination, reverse the ordering to get the last N items
                return ascending
                    ? src.OrderByDescending(orderSelector).ThenByDescending(keySelector)
                    : src.OrderBy(orderSelector).ThenByDescending(keySelector);
            }
        };

        // Create cursor function
        Func<TEntity, string> createCursor = entity =>
            compositeKeyConverter((orderSelector(entity), keySelector(entity)));

        return ExecutePagination<TEntity, TEntityKey>(
            source,
            orderSource,
            first,
            last,
            afterCursor,
            beforeCursor,
            includeTotal,
            createCursor);
    }

    /// <summary>
    /// Creates a cursor-paginated list from an IEnumerable source with ordering by a specified field using query parameters.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key used for cursor-based pagination.</typeparam>
    /// <typeparam name="TOrderKey">The type of the field used for ordering.</typeparam>
    /// <param name="src">The IEnumerable source.</param>
    /// <param name="keySelector">Function to select the key from the entity.</param>
    /// <param name="orderSelector">Function to select the ordering field from the entity.</param>
    /// <param name="compositeKeyConverter">Function to convert the composite key (order value and entity key) to a string cursor.</param>
    /// <param name="compositeCursorConverter">Function to convert a string cursor back to a composite key (order value and entity key).</param>
    /// <param name="queryParameters">The pagination query parameters.</param>
    /// <param name="ascending">Whether to order in ascending order. Default is true.</param>
    /// <returns>A cursor paginated list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when queryParameters is null.</exception>
    public static CursorPaginatedList<TEntity, TEntityKey> ToCursorPaginatedList<TEntity, TEntityKey, TOrderKey>(
        this IEnumerable<TEntity> src,
        Func<TEntity, TEntityKey> keySelector,
        Func<TEntity, TOrderKey> orderSelector,
        Func<(TOrderKey OrderValue, TEntityKey Key), string> compositeKeyConverter,
        Func<string, (TOrderKey OrderValue, TEntityKey Key)> compositeCursorConverter,
        CursorPaginationQueryParameters queryParameters,
        bool ascending = true)
            where TEntity : class
            where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
            where TOrderKey : IComparable<TOrderKey>
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        return src.ToCursorPaginatedList(
            keySelector,
            orderSelector,
            compositeKeyConverter,
            compositeCursorConverter,
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending);
    }

    /// <summary>
    /// Creates a cursor-paginated list from an IEnumerable source of entities with integer IDs, ordered by a specified field.
    /// This method creates composite cursors containing both the order field value and the entity ID.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that implements IIdentifiable with int key.</typeparam>
    /// <typeparam name="TOrderKey">The type of the field used for ordering.</typeparam>
    /// <param name="src">The IEnumerable source.</param>
    /// <param name="orderSelector">Function to select the ordering field from the entity.</param>
    /// <param name="queryParameters">The pagination query parameters.</param>
    /// <param name="ascending">Whether to order in ascending order. Default is true.</param>
    /// <returns>A cursor paginated list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when queryParameters is null.</exception>
    public static CursorPaginatedList<TEntity, int> ToCursorPaginatedList<TEntity, TOrderKey>(
        this IEnumerable<TEntity> src,
        Func<TEntity, TOrderKey> orderSelector,
        CursorPaginationQueryParameters queryParameters,
        bool ascending = true)
            where TEntity : class, IIdentifiable<int>
            where TOrderKey : IComparable<TOrderKey>
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        return src.ToCursorPaginatedList(
            item => item.Id,
            orderSelector,
            CursorConverters.CreateCompositeKeyConverter<TOrderKey, int>(),
            CursorConverters.CreateCompositeCursorConverter<TOrderKey, int>(),
            queryParameters.First,
            queryParameters.Last,
            queryParameters.After,
            queryParameters.Before,
            queryParameters.IncludeTotal,
            ascending);
    }

    /// <summary>
    /// Common helper method for executing cursor pagination logic.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key used for cursor-based pagination.</typeparam>
    /// <param name="src">The filtered IEnumerable source.</param>
    /// <param name="orderSource">Function to order the source based on pagination direction.</param>
    /// <param name="first">Number of items to take from the beginning of the result set.</param>
    /// <param name="last">Number of items to take from the end of the result set.</param>
    /// <param name="afterCursor">Cursor indicating to start after this position.</param>
    /// <param name="beforeCursor">Cursor indicating to end before this position.</param>
    /// <param name="includeTotal">Whether to include the total count of items.</param>
    /// <param name="createCursor">Function to create cursor from entity.</param>
    /// <returns>A cursor paginated list.</returns>
    private static CursorPaginatedList<TEntity, TEntityKey> ExecutePagination<TEntity, TEntityKey>(
        IEnumerable<TEntity> src,
        EnumerableOrderer<TEntity> orderSource,
        int? first,
        int? last,
        string? afterCursor,
        string? beforeCursor,
        bool includeTotal,
        Func<TEntity, string> createCursor)
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

            var orderedSource = orderSource(src, isFirst: true);
            pageList = orderedSource.Take(first.Value + 1).ToList();

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

            var orderedSource = orderSource(src, isFirst: false);
            pageList = orderedSource.Take(last.Value + 1).ToList();

            hasPreviousPage = pageList.Count > last.Value;

            if (hasPreviousPage)
            {
                pageList.RemoveAt(pageList.Count - 1);
            }

            pageList.Reverse();
        }
        else
        {
            var orderedSource = orderSource(src, isFirst: true);
            pageList = orderedSource.ToList();
        }

        var firstPageItem = pageList.FirstOrDefault();
        var lastPageItem = pageList.LastOrDefault();

        return new CursorPaginatedList<TEntity, TEntityKey>(
            pageList,
            hasNextPage,
            hasPreviousPage,
            firstPageItem != null ? createCursor(firstPageItem) : null,
            lastPageItem != null ? createCursor(lastPageItem) : null,
            includeTotal ? src.Count() : null);
    }

}