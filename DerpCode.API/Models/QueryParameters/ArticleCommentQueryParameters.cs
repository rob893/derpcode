namespace DerpCode.API.Models.QueryParameters;

public sealed record ArticleCommentQueryParameters : CursorPaginationQueryParameters
{
    public int? ArticleId { get; init; }

    public int? ParentCommentId { get; init; }

    public int? QuotedCommentId { get; init; }

    public ArticleCommentOrderBy OrderBy { get; init; } = ArticleCommentOrderBy.MostRecent;

    public OrderByDirection OrderByDirection { get; init; } = OrderByDirection.Descending;
}

public enum ArticleCommentOrderBy
{
    MostRecent,
    HighestRated
}