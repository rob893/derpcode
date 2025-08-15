namespace DerpCode.API.Models.QueryParameters;

public sealed record ProblemQueryParameters : CursorPaginationQueryParameters
{
    public bool IncludeUnpublished { get; init; }
}