using System;
using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class RefreshToken : IOwnedByUser<int>
{
    [MaxLength(255)]
    public string DeviceId { get; set; } = string.Empty;

    public int UserId { get; set; }

    public User User { get; set; } = default!;

    [MaxLength(255)]
    public string TokenHash { get; set; } = string.Empty;

    [MaxLength(255)]
    public string TokenSalt { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset Expiration { get; set; }
}