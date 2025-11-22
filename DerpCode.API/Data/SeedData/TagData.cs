using System.Collections.Generic;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Data.SeedData;

public static class TagData
{
    public static readonly IReadOnlyList<Tag> Tags =
    [
        new Tag
        {
            Id = 1,
            Name = "math",
        }
    ];
}