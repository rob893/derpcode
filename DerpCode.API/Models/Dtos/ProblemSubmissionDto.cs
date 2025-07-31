using System;
using System.Collections.Generic;
using System.Linq;
using DerpCode.API.Models.Entities;
using DerpCode.API.Utilities;

namespace DerpCode.API.Models.Dtos;

public sealed record ProblemSubmissionDto : IIdentifiable<long>, IOwnedByUser<int>
{
    public required long Id { get; init; }

    public required int UserId { get; init; }

    public required int ProblemId { get; init; }

    public required LanguageType Language { get; init; }

    public required string Code { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required bool Pass { get; init; }

    public required int TestCaseCount { get; init; }

    public required int PassedTestCases { get; init; }

    public required int FailedTestCases { get; init; }

    public required string ErrorMessage { get; init; }

    public required long ExecutionTimeInMs { get; init; }

    public required List<TestCaseResultDto> TestCaseResults { get; init; }

    public static ProblemSubmissionDto FromEntity(ProblemSubmission submission, bool showPremiumContent, string stdOut)
    {
        ArgumentNullException.ThrowIfNull(submission);

        return new ProblemSubmissionDto
        {
            Id = submission.Id,
            UserId = submission.UserId,
            ProblemId = submission.ProblemId,
            Language = submission.Language,
            Code = submission.Code,
            CreatedAt = submission.CreatedAt,
            Pass = submission.Pass,
            TestCaseCount = submission.TestCaseCount,
            PassedTestCases = submission.PassedTestCases,
            FailedTestCases = submission.FailedTestCases,
            ErrorMessage = submission.ErrorMessage ?? string.Empty,
            ExecutionTimeInMs = submission.ExecutionTimeInMs,
            TestCaseResults = showPremiumContent ? [.. submission.TestCaseResults.Select(result =>
            {
                var startDeliminator = $"|derpcode-start-test-{result.TestCaseIndex}|";
                var endDeliminator = $"|derpcode-end-test-{result.TestCaseIndex}|";
                var stdIoForProblem = UtilityFunctions.GetStringBetween(stdOut, startDeliminator, endDeliminator);

                return TestCaseResultDto.FromEntity(result, stdIoForProblem.Trim());
            })] : []
        };
    }
}