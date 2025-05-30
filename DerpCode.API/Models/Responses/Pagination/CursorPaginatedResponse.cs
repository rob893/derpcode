using System;
using System.Collections.Generic;

namespace DerpCode.API.Models.Responses.Pagination;

/// <summary>
/// Object representing a cursor paginated response.
/// </summary>
/// <typeparam name="TEntity">Type of entity.</typeparam>
/// <typeparam name="TEntityKey">Type of entity key.</typeparam>
public record CursorPaginatedResponse<TEntity, TEntityKey>
    where TEntity : class
    where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
{
    /// <summary>
    /// Gets the edges.
    /// </summary>
    public IEnumerable<CursorPaginatedResponseEdge<TEntity>>? Edges { get; init; }

    /// <summary>
    /// Gets the nodes.
    /// </summary>
    public IEnumerable<TEntity>? Nodes { get; init; }

    /// <summary>
    /// Gets the page info.
    /// </summary>
    public CursorPaginatedResponsePageInfo PageInfo { get; init; } = default!;
}

public sealed record CursorPaginatedResponse<TEntity> : CursorPaginatedResponse<TEntity, int>
    where TEntity : class
{ }