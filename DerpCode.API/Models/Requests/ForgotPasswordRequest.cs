using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Requests;

public sealed record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = default!;
}