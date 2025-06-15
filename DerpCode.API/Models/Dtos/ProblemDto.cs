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

    public required ArticleDto? ExplanationArticle { get; init; }

    public required ProblemDifficulty Difficulty { get; init; }

    public required List<object> ExpectedOutput { get; init; }

    public required List<object> Input { get; init; }

    public required List<string> Hints { get; init; } = [];

    public required List<TagDto> Tags { get; init; }

    public required List<ProblemDriverDto> Drivers { get; init; }

    public static ProblemDto FromEntity(Problem problem, bool isCurrentUserAdmin, bool isCurrentUserPremium)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return new ProblemDto
        {
            Id = problem.Id,
            Name = problem.Name,
            Description = problem.Description,
            Difficulty = problem.Difficulty,
            ExpectedOutput = problem.ExpectedOutput,
            ExplanationArticle = isCurrentUserAdmin || isCurrentUserPremium ? ArticleDto.FromEntity(problem.ExplanationArticle) : null,
            Hints = [.. problem.Hints],
            Input = problem.Input,
            Tags = [.. problem.Tags.Select(TagDto.FromEntity)],
            Drivers = [.. problem.Drivers.Select(x => ProblemDriverDto.FromEntity(x, isCurrentUserAdmin))]
        };
    }
}