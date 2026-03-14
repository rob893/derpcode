using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

/// <summary>
/// Data transfer object for experience events (XP history).
/// </summary>
public sealed record ExperienceEventDto : IIdentifiable<long>, IOwnedByUser<int>
{
    /// <summary>
    /// Gets the unique identifier of this event.
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// Gets the user who earned this XP.
    /// </summary>
    public required int UserId { get; init; }

    /// <summary>
    /// Gets the type of event that occurred.
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// Gets the source type (e.g., Problem).
    /// </summary>
    public required string SourceType { get; init; }

    /// <summary>
    /// Gets the source identifier (e.g., problem ID).
    /// </summary>
    public required string? SourceId { get; init; }

    /// <summary>
    /// Gets the XP change from this event.
    /// </summary>
    public required int XpDelta { get; init; }

    /// <summary>
    /// Gets the event metadata as a JSON string.
    /// </summary>
    public required string Metadata { get; init; }

    /// <summary>
    /// Gets when this event occurred.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Creates a DTO from an ExperienceEvent entity.
    /// </summary>
    /// <param name="entity">The entity to convert.</param>
    /// <returns>The DTO representation.</returns>
    public static ExperienceEventDto FromEntity(ExperienceEvent entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ExperienceEventDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            EventType = entity.EventType.ToString(),
            SourceType = entity.SourceType.ToString(),
            SourceId = entity.SourceId,
            XpDelta = entity.XpDelta,
            Metadata = entity.Metadata,
            CreatedAt = entity.CreatedAt
        };
    }
}
