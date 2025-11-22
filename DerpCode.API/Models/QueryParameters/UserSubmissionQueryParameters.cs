namespace DerpCode.API.Models.QueryParameters;

public sealed record UserSubmissionQueryParameters : CursorPaginationQueryParameters
{
    public int? ProblemId { get; init; }
}