using System;

namespace DerpCode.API.Models.Entities;

/// <summary>
/// Represents an achievement earned by a user.
/// </summary>
public sealed class UserAchievement : IIdentifiable<long>, IOwnedByUser<int>
{
    /// <summary>
    /// Gets or sets the unique identifier of this achievement record.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the user who earned this achievement.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user navigation property.
    /// </summary>
    public User User { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of achievement earned.
    /// </summary>
    public AchievementType AchievementType { get; set; }

    /// <summary>
    /// Gets or sets when this achievement was earned.
    /// </summary>
    public DateTimeOffset EarnedAt { get; set; } = DateTimeOffset.UtcNow;
}
