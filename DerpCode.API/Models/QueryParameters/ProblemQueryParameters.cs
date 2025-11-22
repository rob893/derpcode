using System.Collections.Generic;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.QueryParameters;

public sealed record ProblemQueryParameters : CursorPaginationQueryParameters
{
    public bool IncludeUnpublished { get; init; }

    public IReadOnlyList<ProblemDifficulty>? Difficulties { get; init; }

    public IReadOnlyList<string>? Tags { get; init; }

    public ProblemOrderBy OrderBy { get; init; } = ProblemOrderBy.Name;

    public OrderByDirection OrderByDirection { get; init; } = OrderByDirection.Descending;
}

public enum ProblemOrderBy
{
    Difficulty,
    Name
}