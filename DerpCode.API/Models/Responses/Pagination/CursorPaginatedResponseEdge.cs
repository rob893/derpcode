namespace DerpCode.API.Models.Responses.Pagination;

/// <summary>
/// Object representing an edge (a value and its cursor).
/// </summary>
/// <typeparam name="TEntity">Type of entity.</typeparam>
public sealed record CursorPaginatedResponseEdge<TEntity>
{
    /// <summary>
    /// Gets the cursor.
    /// </summary>
    public required string Cursor { get; init; }

    /// <summary>
    /// Gets the node.
    /// </summary>
    public required TEntity Node { get; init; }
}