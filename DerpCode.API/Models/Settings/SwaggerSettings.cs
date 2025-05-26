using System.Collections.Generic;

namespace DerpCode.API.Models.Settings;

public record SwaggerSettings
{
    public SwaggerAuthSettings AuthSettings { get; init; } = default!;

    public bool Enabled { get; init; }

    public List<string> SupportedApiVersions { get; init; } = [];
}