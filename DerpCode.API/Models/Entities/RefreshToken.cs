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
    public string Token { get; set; } = string.Empty;

    public DateTimeOffset Expiration { get; set; }
}