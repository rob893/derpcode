using System.Collections.Generic;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.QueryParameters;

public sealed record ProblemQueryParameters : CursorPaginationQueryParameters
{
    public bool IncludeUnpublished { get; init; }

    public List<ProblemDifficulty>? Difficulties { get; init; }

    public List<string>? Tags { get; init; }
}