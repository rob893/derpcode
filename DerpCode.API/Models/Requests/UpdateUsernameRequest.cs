using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Requests;

public sealed record UpdateUsernameRequest
{
    [Required]
    [MinLength(1)]
    public string NewUsername { get; init; } = default!;
}