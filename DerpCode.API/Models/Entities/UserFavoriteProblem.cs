using System;

namespace DerpCode.API.Models.Entities;

public sealed class UserFavoriteProblem
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int ProblemId { get; set; }

    public Problem Problem { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}