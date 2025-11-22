using System;
using System.Collections.Generic;
using System.Linq;

namespace DerpCode.API.Models.Dtos;

public sealed record UserDto : IIdentifiable<int>
{
    public required int Id { get; init; }

    public required string UserName { get; init; }

    public required string Email { get; init; }

    public required bool EmailConfirmed { get; init; }

    public required DateTimeOffset Created { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    public required IReadOnlyList<LinkedAccountDto> LinkedAccounts { get; init; }

    public required DateTimeOffset? LastLogin { get; init; }

    public required DateTimeOffset LastPasswordChange { get; init; }

    public required DateTimeOffset LastEmailChange { get; init; }

    public required DateTimeOffset LastUsernameChange { get; init; }

    public required DateTimeOffset? LastEmailConfirmationSent { get; init; }

    public static UserDto FromEntity(Entities.User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            Created = user.Created,
            LastLogin = user.LastLogin,
            LastPasswordChange = user.LastPasswordChange,
            LastEmailChange = user.LastEmailChange,
            LastUsernameChange = user.LastUsernameChange,
            LastEmailConfirmationSent = user.LastEmailConfirmationSent,
            Roles = [.. user.UserRoles.Select(x => x.Role).Select(role => role.Name ?? string.Empty)],
            LinkedAccounts = [.. user.LinkedAccounts.Select(LinkedAccountDto.FromEntity)]
        };
    }
}