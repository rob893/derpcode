using System.Collections.Generic;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.QueryParameters;

public record ProblemQueryParameters : CursorPaginationQueryParameters
{
    public bool IncludeUnpublished { get; init; }

    public string? SearchTerm { get; init; }

    public IReadOnlyList<ProblemDifficulty>? Difficulties { get; init; }

    public IReadOnlyList<string>? Tags { get; init; }

    public ProblemOrderBy OrderBy { get; init; } = ProblemOrderBy.Name;

    public OrderByDirection OrderByDirection { get; init; } = OrderByDirection.Descending;
}

public sealed record PersonalizedProblemListQueryParameters : ProblemQueryParameters
{
    public bool? IsFavorite { get; init; }

    public bool? HasAttempted { get; init; }

    public bool? HasPassed { get; init; }
}

public enum ProblemOrderBy
{
    Difficulty,
    Name
}