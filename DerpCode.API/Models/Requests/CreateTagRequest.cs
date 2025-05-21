using System.ComponentModel.DataAnnotations;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests;

public sealed record CreateTagRequest
{
    [MaxLength(63)]
    [MinLength(1)]
    [Required]
    public string Name { get; init; } = string.Empty;

    public Tag ToEntity()
    {
        return new Tag
        {
            Name = this.Name
        };
    }
}