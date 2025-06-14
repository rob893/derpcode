using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class Problem : IIdentifiable<int>
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    [MaxLength(15)]
    public ProblemDifficulty Difficulty { get; set; }

    public List<object> ExpectedOutput { get; set; } = [];

    public List<object> Input { get; set; } = [];

    public List<string> Hints { get; set; } = [];

    public List<Tag> Tags { get; set; } = [];

    public List<ProblemDriver> Drivers { get; set; } = [];

    public List<ProblemSubmission> ProblemSubmissions { get; set; } = [];
}