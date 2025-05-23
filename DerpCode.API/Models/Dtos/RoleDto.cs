
using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record RoleDto : IIdentifiable<int>
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string NormalizedName { get; init; }

    public static RoleDto FromEntity(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            NormalizedName = role.NormalizedName ?? string.Empty
        };
    }
}