namespace DerpCode.API.Models.Settings;

public sealed record PostgresSettings
{
    public string DefaultConnection { get; init; } = default!;

    public bool EnableSensitiveDataLogging { get; init; }

    public bool EnableDetailedErrors { get; init; }
}