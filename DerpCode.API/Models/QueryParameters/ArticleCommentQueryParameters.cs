namespace DerpCode.API.Models.QueryParameters;

public record ArticleCommentQueryParameters : CursorPaginationQueryParameters
{
    public int? ArticleId { get; init; }
}