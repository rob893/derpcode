using System;
using System.Collections.Generic;
using System.Linq;
using DerpCode.API.Models.Entities;
using DerpCode.API.Utilities;

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

    public required int TotalXp { get; init; }

    public required int Level { get; init; }

    public required int XpIntoLevel { get; init; }

    public required int XpForNextLevel { get; init; }

    public static UserDto FromEntity(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var totalXp = user.Progress?.TotalXp ?? 0;
        var levelProgress = ProgressionMath.GetLevelProgress(totalXp);

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
            TotalXp = totalXp,
            Level = user.Progress?.Level ?? levelProgress.Level,
            XpIntoLevel = levelProgress.XpIntoLevel,
            XpForNextLevel = levelProgress.XpForNextLevel,
            Roles = [.. user.UserRoles.Select(x => x.Role).Select(role => role.Name ?? string.Empty)],
            LinkedAccounts = [.. user.LinkedAccounts.Select(LinkedAccountDto.FromEntity)]
        };
    }
}
