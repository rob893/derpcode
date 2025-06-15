using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class ProblemSubmission : IIdentifiable<long>, IOwnedByUser<int>
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = default!;

    public int ProblemId { get; set; }

    public Problem Problem { get; set; } = default!;

    [MaxLength(50)]
    public LanguageType Language { get; set; }

    public string Code { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public bool Pass { get; set; }

    public int TestCaseCount { get; set; }

    public int PassedTestCases { get; set; }

    public int FailedTestCases { get; set; }

    public string? ErrorMessage { get; set; }

    public int ExecutionTimeInMs { get; set; }

    public int? MemoryKb { get; set; }

    public List<TestCaseResult> TestCaseResults { get; set; } = [];
}