using System.Collections.Generic;

namespace DerpCode.API.Models.Settings;

public sealed record SwaggerSettings
{
    public SwaggerAuthSettings AuthSettings { get; init; } = default!;

    public bool Enabled { get; init; }

    public IReadOnlyList<string> SupportedApiVersions { get; init; } = [];
}