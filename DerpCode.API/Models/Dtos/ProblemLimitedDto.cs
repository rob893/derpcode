using System;
using System.Collections.Generic;
using System.Linq;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record ProblemLimitedDto : IIdentifiable<int>
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required bool IsPublished { get; init; }

    public required ProblemDifficulty Difficulty { get; init; }

    public required IReadOnlyList<TagDto> Tags { get; init; }

    public static ProblemLimitedDto FromEntity(Problem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return new ProblemLimitedDto
        {
            Id = problem.Id,
            Name = problem.Name,
            Difficulty = problem.Difficulty,
            IsPublished = problem.IsPublished,
            Tags = [.. problem.Tags.Select(TagDto.FromEntity)]
        };
    }
}