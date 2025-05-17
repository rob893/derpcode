using System.Collections.Generic;

namespace DerpCode.API.Models;

public sealed class Problem
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Difficulty { get; set; } = string.Empty;

    public IReadOnlyList<object> ExpectedOutput { get; set; } = [];

    public IReadOnlyList<object> Tags { get; set; } = [];

    public IReadOnlyList<object> Input { get; set; } = [];

    public IReadOnlyList<ProblemDriver> Drivers { get; set; } = [];
}
