using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record TestCaseResultDto : IIdentifiable<string>
{
    public required string Id { get; init; }

    public required int ProblemSubmissionId { get; init; }

    public required int TestCaseIndex { get; init; }

    public required bool Pass { get; init; }

    public required string StdOut { get; init; }

    public required string? ErrorMessage { get; init; }

    public required int ExecutionTimeInMs { get; init; }

    public required int? MemoryKb { get; init; }

    public required object Input { get; init; }

    public required object ExpectedOutput { get; init; }

    public required object ActualOutput { get; init; }

    public required bool IsHidden { get; init; }

    public static TestCaseResultDto FromEntity(TestCaseResult result, string stdOut)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new TestCaseResultDto
        {
            Id = result.Id,
            ProblemSubmissionId = result.ProblemSubmissionId,
            TestCaseIndex = result.TestCaseIndex,
            Pass = result.Pass,
            ErrorMessage = result.ErrorMessage ?? string.Empty,
            ExecutionTimeInMs = result.ExecutionTimeInMs,
            MemoryKb = result.MemoryKb,
            Input = result.Input,
            StdOut = stdOut,
            ExpectedOutput = result.ExpectedOutput,
            ActualOutput = result.ActualOutput,
            IsHidden = result.IsHidden
        };
    }
}