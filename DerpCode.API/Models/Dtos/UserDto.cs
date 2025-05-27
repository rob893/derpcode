using System;
using System.Collections.Generic;
using System.Linq;

namespace DerpCode.API.Models.Dtos;

public sealed record UserDto : IIdentifiable<int>
{
    public required int Id { get; init; }

    public required string UserName { get; init; }

    public required string Email { get; init; }

    public required DateTimeOffset Created { get; init; }

    public required List<string> Roles { get; init; }

    public required List<LinkedAccountDto> LinkedAccounts { get; init; }

    public static UserDto FromEntity(Entities.User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Created = user.Created,
            Roles = [.. user.UserRoles.Select(x => x.Role).Select(role => role.Name)],
            LinkedAccounts = [.. user.LinkedAccounts.Select(LinkedAccountDto.FromEntity)]
        };
    }
}