namespace DerpCode.API.Models.QueryParameters;

public record UserSubmissionQueryParameters : CursorPaginationQueryParameters
{
    public int? ProblemId { get; init; }
}