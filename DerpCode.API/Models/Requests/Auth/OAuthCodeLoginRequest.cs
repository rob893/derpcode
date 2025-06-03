using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Requests.Auth;

/// <summary>
/// Request model for OAuth login
/// </summary>
public sealed record OAuthCodeLoginRequest
{
    /// <summary>
    /// Code to exchange for an access token
    /// </summary>
    [Required]
    public string Code { get; init; } = default!;

    /// <summary>
    /// Unique device identifier for this login session
    /// </summary>
    [Required]
    public string DeviceId { get; init; } = default!;
}
