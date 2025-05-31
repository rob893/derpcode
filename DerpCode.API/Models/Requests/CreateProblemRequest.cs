using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests;

public sealed record CreateProblemRequest
{
    [MaxLength(255)]
    [MinLength(1)]
    [Required]
    public string Name { get; init; } = string.Empty;

    [MinLength(1)]
    [Required]
    public string Description { get; init; } = string.Empty;

    [Required]
    public ProblemDifficulty? Difficulty { get; init; }

    [MinLength(1)]
    [Required]
    public List<object> ExpectedOutput { get; init; } = [];

    [MinLength(1)]
    [Required]
    public List<object> Input { get; init; } = [];

    public List<string> Hints { get; init; } = [];

    [MinLength(1)]
    [Required]
    public List<CreateTagRequest> Tags { get; init; } = [];

    [MinLength(1)]
    [Required]
    public List<CreateProblemDriverRequest> Drivers { get; init; } = [];

    public Problem ToEntity()
    {
        return new Problem
        {
            Name = this.Name,
            Description = this.Description,
            Difficulty = this.Difficulty ?? throw new InvalidOperationException("Difficulty is required"),
            ExpectedOutput = this.ExpectedOutput,
            Hints = [.. this.Hints],
            Input = this.Input,
            Tags = [.. this.Tags.Select(tag => tag.ToEntity())],
            Drivers = [.. this.Drivers.Select(driver => driver.ToEntity())]
        };
    }

    public static CreateProblemRequest FromEntity(Problem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return new CreateProblemRequest
        {
            Name = problem.Name,
            Description = problem.Description,
            Difficulty = problem.Difficulty,
            ExpectedOutput = [.. problem.ExpectedOutput],
            Input = [.. problem.Input],
            Hints = [.. problem.Hints],
            Tags = [.. problem.Tags.Select(CreateTagRequest.FromEntity)],
            Drivers = [.. problem.Drivers.Select(CreateProblemDriverRequest.FromEntity)]
        };
    }
}