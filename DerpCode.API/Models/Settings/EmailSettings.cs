using System;

namespace DerpCode.API.Models.Settings;

public sealed record EmailSettings
{
    public bool Enabled { get; init; }

    public string FromAddress { get; init; } = default!;

    public Uri AcsEndpoint { get; init; } = default!;
}