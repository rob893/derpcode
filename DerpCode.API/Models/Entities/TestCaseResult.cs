using System;

namespace DerpCode.API.Models.Entities;

public sealed class TestCaseResult : IIdentifiable<string>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public int ProblemSubmissionId { get; set; }

    public ProblemSubmission ProblemSubmission { get; set; } = default!;

    public int TestCaseIndex { get; set; }

    public bool Pass { get; init; }

    public string? ErrorMessage { get; init; }

    public int ExecutionTimeInMs { get; init; }

    public int? MemoryKb { get; set; }

    public object Input { get; set; } = default!;

    public object ExpectedOutput { get; set; } = default!;

    public object ActualOutput { get; set; } = default!;

    public bool IsHidden { get; set; }
}
