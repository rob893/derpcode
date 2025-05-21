using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record TagDto : IIdentifiable<int>
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public static TagDto FromEntity(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}