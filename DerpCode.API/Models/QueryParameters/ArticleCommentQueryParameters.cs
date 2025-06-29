namespace DerpCode.API.Models.QueryParameters;

public record ArticleCommentQueryParameters : CursorPaginationQueryParameters
{
    public int? ArticleId { get; init; }

    public int? ParentCommentId { get; init; }

    public int? QuotedCommentId { get; init; }
}