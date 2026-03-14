namespace DerpCode.API.Models.Entities;

/// <summary>
/// Types of achievements a user can earn.
/// </summary>
public enum AchievementType
{
    /// <summary>
    /// Solved their first problem.
    /// </summary>
    FirstSolve,

    /// <summary>
    /// Solved 10 problems.
    /// </summary>
    TenSolves,

    /// <summary>
    /// Solved 50 problems.
    /// </summary>
    FiftySolves,

    /// <summary>
    /// Solved 100 problems.
    /// </summary>
    HundredSolves,

    /// <summary>
    /// Solved a problem with a perfect score (max XP).
    /// </summary>
    PerfectScore,

    /// <summary>
    /// Reached level 5.
    /// </summary>
    ReachedLevel5,

    /// <summary>
    /// Reached level 10.
    /// </summary>
    ReachedLevel10,

    /// <summary>
    /// Reached level 25.
    /// </summary>
    ReachedLevel25,

    /// <summary>
    /// Solved a VeryHard problem.
    /// </summary>
    VeryHardSolver,

    /// <summary>
    /// Solved a problem in every difficulty category.
    /// </summary>
    AllDifficulties
}
