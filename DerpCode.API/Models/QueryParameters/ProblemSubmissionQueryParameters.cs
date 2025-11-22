namespace DerpCode.API.Models.QueryParameters;

public sealed record ProblemSubmissionQueryParameters : CursorPaginationQueryParameters
{
    public int? ProblemId { get; init; }

    public int? UserId { get; init; }
}