using System;
using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class UserProgress : IIdentifiable<int>
{
    public int Id => this.UserId;

    public int UserId { get; set; }

    public User User { get; set; } = default!;

    public int TotalXp { get; set; }

    public int Level { get; set; } = 1;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Postgres xmin system column used as an optimistic concurrency token.
    /// </summary>
    [Timestamp]
    public uint RowVersion { get; set; }
}
