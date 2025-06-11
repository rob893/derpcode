using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record ProblemSubmissionDto
{
    public bool Pass { get; init; }

    public int TestCaseCount { get; init; }

    public int PassedTestCases { get; init; }

    public int FailedTestCases { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public long ExecutionTimeInMs { get; init; }

    public static ProblemSubmissionDto FromEntity(ProblemSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);

        return new ProblemSubmissionDto
        {
            Pass = submission.Pass,
            TestCaseCount = submission.TestCaseCount,
            PassedTestCases = submission.PassedTestCases,
            FailedTestCases = submission.FailedTestCases,
            ErrorMessage = submission.ErrorMessage ?? string.Empty,
            ExecutionTimeInMs = submission.ExecutionTimeInMs
        };
    }
}