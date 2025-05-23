using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Requests.Auth;

public sealed record RefreshTokenRequest
{
    [Required]
    public string Token { get; init; } = default!;

    [Required]
    public string RefreshToken { get; init; } = default!;

    [Required]
    public string DeviceId { get; init; } = default!;
}