using System;
using System.ComponentModel.DataAnnotations;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests;

public sealed record PatchUserPreferencesRequest
{
    [Required]
    public PreferencesDto Preferences { get; init; } = default!;

    public static PatchUserPreferencesRequest FromEntity(UserPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        return new PatchUserPreferencesRequest
        {
            Preferences = PreferencesDto.FromEntity(preferences.Preferences)
        };
    }
}