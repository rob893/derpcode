using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class Tag : IIdentifiable<int>
{
    public int Id { get; set; }

    [MaxLength(63)]
    public string Name { get; set; } = string.Empty;

    public List<Problem> Problems { get; set; } = [];

    public List<Article> Articles { get; set; } = [];
}