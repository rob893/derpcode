using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Requests.Auth;

public sealed record LoginRequest
{
    [Required]
    public string Username { get; init; } = default!;

    [Required]
    public string Password { get; init; } = default!;

    [Required]
    public string DeviceId { get; init; } = default!;
}