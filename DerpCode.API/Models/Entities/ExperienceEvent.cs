using System;
using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class ExperienceEvent : IIdentifiable<long>, IOwnedByUser<int>
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = default!;

    [MaxLength(50)]
    public ExperienceEventType EventType { get; set; }

    [MaxLength(50)]
    public ExperienceEventSourceType SourceType { get; set; }

    [MaxLength(128)]
    public string? SourceId { get; set; }

    public int XpDelta { get; set; }

    [MaxLength(255)]
    public string IdempotencyKey { get; set; } = string.Empty;

    public string Metadata { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
