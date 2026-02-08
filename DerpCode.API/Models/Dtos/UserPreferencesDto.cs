using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record UserPreferencesDto : IIdentifiable<int>, IOwnedByUser<int>
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required DateTimeOffset LastUpdated { get; init; }

    public required PreferencesDto Preferences { get; init; }

    public static UserPreferencesDto FromEntity(UserPreferences obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return new UserPreferencesDto
        {
            Id = obj.Id,
            UserId = obj.UserId,
            LastUpdated = obj.LastUpdated,
            Preferences = PreferencesDto.FromEntity(obj.Preferences)
        };
    }
}

public sealed record PreferencesDto
{
    public required UserUIPreferenceDto UIPreference { get; init; }

    public required UserCodePreferenceDto CodePreference { get; init; }

    public required UserEditorPreferenceDto EditorPreference { get; init; }

    public static PreferencesDto FromEntity(Preferences obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return new PreferencesDto
        {
            CodePreference = UserCodePreferenceDto.FromEntity(obj.CodePreference),
            EditorPreference = UserEditorPreferenceDto.FromEntity(obj.EditorPreference),
            UIPreference = UserUIPreferenceDto.FromEntity(obj.UIPreference)
        };
    }
}

public sealed record UserEditorPreferenceDto
{
    public required bool EnableFlameEffects { get; init; } = true;

    public static UserEditorPreferenceDto FromEntity(UserEditorPreference obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return new UserEditorPreferenceDto
        {
            EnableFlameEffects = obj.EnableFlameEffects
        };
    }
}

public sealed record UserCodePreferenceDto
{
    public required LanguageType DefaultLanguage { get; init; } = LanguageType.JavaScript;

    public static UserCodePreferenceDto FromEntity(UserCodePreference obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return new UserCodePreferenceDto
        {
            DefaultLanguage = obj.DefaultLanguage
        };
    }
}

public sealed record UserUIPreferenceDto
{
    public required UITheme UITheme { get; init; } = UITheme.Dark;

    public required int PageSize { get; init; } = 5;

    public static UserUIPreferenceDto FromEntity(UserUIPreference obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return new UserUIPreferenceDto
        {
            PageSize = obj.PageSize,
            UITheme = obj.UITheme
        };
    }
}