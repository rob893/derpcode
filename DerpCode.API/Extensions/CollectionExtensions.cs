using System;
using System.Collections.Generic;
using System.Linq;
using DerpCode.API.Core;
using DerpCode.API.Models;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Models.Responses.Pagination;

namespace DerpCode.API.Extensions;

public static class CollectionExtensions
{
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
}