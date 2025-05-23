using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Requests.Auth;

public sealed record GoogleLoginRequest
{
    [Required]
    public string IdToken { get; init; } = default!;

    [Required]
    public string DeviceId { get; init; } = default!;
}