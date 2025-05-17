using System.Collections.Generic;

namespace DerpCode.API.Models;

public sealed class Problem
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Difficulty { get; set; } = string.Empty;

    public List<object> ExpectedOutput { get; set; } = [];

    public List<object> Tags { get; set; } = [];

    public List<object> Input { get; set; } = [];

    public List<ProblemDriver> Drivers { get; set; } = [];
}
