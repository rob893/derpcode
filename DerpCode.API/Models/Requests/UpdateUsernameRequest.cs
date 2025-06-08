using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Requests;

public sealed record UpdateUsernameRequest
{
    [Required]
    public string Password { get; init; } = default!;

    [Required]
    [MinLength(1)]
    public string NewUsername { get; init; } = default!;
}