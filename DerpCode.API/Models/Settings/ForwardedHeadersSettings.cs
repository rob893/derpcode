using System.Collections.Generic;

namespace DerpCode.API.Models.Settings;

/// <summary>
/// Optional bindings for the ForwardedHeaders middleware. The middleware is wired up unconditionally
/// in <c>Program.cs</c> with a safe loopback-only default; this class lets ops add additional
/// trusted proxy addresses or networks (e.g., an Azure Front Door egress range) via configuration
/// without redeploying the binary.
/// </summary>
public sealed record ForwardedHeadersSettings
{
    /// <summary>
    /// Additional IPv4 or IPv6 proxy addresses whose forwarded headers should be trusted, in
    /// addition to the loopback defaults (<c>127.0.0.1</c>, <c>::1</c>).
    /// </summary>
    public IReadOnlyList<string> KnownProxies { get; init; } = [];

    /// <summary>
    /// Additional IPv4 or IPv6 CIDR ranges whose forwarded headers should be trusted, in addition
    /// to the loopback defaults (<c>127.0.0.0/8</c>, <c>::1/128</c>).
    /// </summary>
    public IReadOnlyList<string> KnownNetworks { get; init; } = [];

    /// <summary>
    /// Maximum number of forwarded entries to honour from the X-Forwarded-* chain. Defaults to 1 to
    /// match the ASP.NET default and avoid trusting deeper hops that may have been added by
    /// upstream clients.
    /// </summary>
    public int ForwardLimit { get; init; } = 1;
}
