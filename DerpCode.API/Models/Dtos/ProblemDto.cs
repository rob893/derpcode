using System;
using System.Collections.Generic;
using System.Linq;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record ProblemDto : IIdentifiable<int>
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required bool IsPublished { get; init; }

    public required ArticleDto? ExplanationArticle { get; init; }

    public required ProblemDifficulty Difficulty { get; init; }

    public required IReadOnlyList<object> ExpectedOutput { get; init; }

    public required IReadOnlyList<object> Input { get; init; }

    public required IReadOnlyList<string> Hints { get; init; } = [];

    public required IReadOnlyList<TagDto> Tags { get; init; }

    public required IReadOnlyList<ProblemDriverDto> Drivers { get; init; }

    public static ProblemDto FromEntity(Problem problem, bool isCurrentUserAdmin, bool isCurrentUserPremium)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return new ProblemDto
        {
            Id = problem.Id,
            Name = problem.Name,
            Description = problem.Description,
            Difficulty = problem.Difficulty,
            IsPublished = problem.IsPublished,
            ExpectedOutput = isCurrentUserAdmin || isCurrentUserPremium ? problem.ExpectedOutput : [],
            ExplanationArticle = ArticleDto.FromEntity(problem.ExplanationArticle),
            Hints = [.. problem.Hints],
            Input = isCurrentUserAdmin || isCurrentUserPremium ? problem.Input : [],
            Tags = [.. problem.Tags.Select(TagDto.FromEntity)],
            Drivers = [.. problem.Drivers.Select(x => ProblemDriverDto.FromEntity(x, isCurrentUserAdmin))]
        };
    }
}