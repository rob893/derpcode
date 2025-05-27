using System.ComponentModel.DataAnnotations;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests.Auth;

public sealed record RegisterUserRequest
{
    [Required]
    public string UserName { get; init; } = default!;

    [Required]
    [StringLength(256, MinimumLength = 6, ErrorMessage = "You must specify a password between 4 and 256 characters")]
    public string Password { get; init; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = default!;

    [Required]
    public string DeviceId { get; init; } = default!;

    public User ToEntity()
    {
        return new User
        {
            UserName = this.UserName,
            Email = this.Email
        };
    }
}