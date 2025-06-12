namespace DerpCode.API.Models.QueryParameters;

public record ProblemSubmissionQueryParameters : CursorPaginationQueryParameters
{
    public int? ProblemId { get; init; }

    public int? UserId { get; init; }
}