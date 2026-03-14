using System;
using DerpCode.API.Utilities;

namespace DerpCode.API.Models.Dtos;

/// <summary>
/// Represents the XP outcome of a submission.
/// </summary>
public sealed record XpResult
{
    /// <summary>
    /// Gets the XP earned by this submission (before delta comparison with best).
    /// </summary>
    public required int XpEarnedThisSubmission { get; init; }

    /// <summary>
    /// Gets the actual XP delta applied to the user's total (earned minus previous best, if improved).
    /// </summary>
    public required int XpDeltaApplied { get; init; }

    /// <summary>
    /// Gets the user's best XP score for this problem across all cycles.
    /// </summary>
    public required int ProblemBestXp { get; init; }

    /// <summary>
    /// Gets the user's total XP after this submission.
    /// </summary>
    public required int TotalXp { get; init; }

    /// <summary>
    /// Gets the user's current level after this submission.
    /// </summary>
    public required int Level { get; init; }

    /// <summary>
    /// Gets the XP progress into the current level.
    /// </summary>
    public required int XpIntoLevel { get; init; }

    /// <summary>
    /// Gets the total XP required to reach the next level from the current level.
    /// </summary>
    public required int XpForNextLevel { get; init; }

    /// <summary>
    /// Gets when the user is next eligible to earn XP for this problem.
    /// </summary>
    public required DateTimeOffset? NextEligibleAt { get; init; }

    /// <summary>
    /// Gets whether this submission was eligible to earn XP.
    /// </summary>
    public required bool IsXpEligibleThisSubmission { get; init; }

    /// <summary>
    /// Gets a default empty XP result for non-XP scenarios (errors, run-only, etc.).
    /// </summary>
    public static XpResult Empty { get; } = new XpResult
    {
        XpEarnedThisSubmission = 0,
        XpDeltaApplied = 0,
        ProblemBestXp = 0,
        TotalXp = 0,
        Level = 1,
        XpIntoLevel = 0,
        XpForNextLevel = ProgressionMath.GetXpToReachLevel(2),
        NextEligibleAt = null,
        IsXpEligibleThisSubmission = false
    };
}
