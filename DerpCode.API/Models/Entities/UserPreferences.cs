using System;

namespace DerpCode.API.Models.Entities;

public sealed class UserPreferences : IIdentifiable<int>, IOwnedByUser<int>
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = default!;

    public Preferences Preferences { get; set; } = new();

    public DateTimeOffset LastUpdated { get; set; }
}

public sealed class Preferences
{
    public UserUIPreference UIPreference { get; set; } = new();

    public UserCodePreference CodePreference { get; set; } = new();

    public UserEditorPreference EditorPreference { get; set; } = new();
}

public sealed class UserEditorPreference
{
    public bool EnableFlameEffects { get; set; } = true;
}

public sealed class UserCodePreference
{
    public LanguageType DefaultLanguage { get; set; } = LanguageType.JavaScript;
}

public sealed class UserUIPreference
{
    public UITheme UITheme { get; set; } = UITheme.Dark;

    public int PageSize { get; set; } = 5;
}

public enum UITheme
{
    Dark,
    Light,
    Custom
}