namespace DerpCode.API.Models;

public sealed record SubmissionResult
{
    public bool Pass { get; init; }

    public int TestCaseCount { get; init; }

    public int PassedTestCases { get; init; }

    public int FailedTestCases { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public long ExecutionTimeInMs { get; init; }
}
