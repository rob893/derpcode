using System;

namespace DerpCode.API.Utilities;

public readonly record struct LevelProgress(int Level, int XpIntoLevel, int XpForNextLevel);

public static class ProgressionMath
{
    public static int GetXpToReachLevel(int level, int levelBaseXp = 100)
    {
        if (level <= 1)
        {
            return 0;
        }

        var levelOffset = level - 1;
        return levelBaseXp * levelOffset * levelOffset;
    }

    public static LevelProgress GetLevelProgress(int totalXp, int levelBaseXp = 100)
    {
        var normalizedTotalXp = Math.Max(totalXp, 0);
        var level = 1;

        while (GetXpToReachLevel(level + 1, levelBaseXp) <= normalizedTotalXp)
        {
            level++;
        }

        var currentLevelFloor = GetXpToReachLevel(level, levelBaseXp);
        var nextLevelFloor = GetXpToReachLevel(level + 1, levelBaseXp);

        return new LevelProgress(
            level,
            normalizedTotalXp - currentLevelFloor,
            nextLevelFloor - currentLevelFloor);
    }

    public static int GetAnchoredMonthlyCycleIndex(DateTimeOffset anchor, DateTimeOffset at)
    {
        if (at <= anchor)
        {
            return 0;
        }

        var index = 0;
        while (anchor.AddMonths(index + 1) <= at)
        {
            index++;
        }

        return index;
    }

    public static DateTimeOffset GetNextCycleStart(DateTimeOffset anchor, int currentCycleIndex)
    {
        return anchor.AddMonths(currentCycleIndex + 1);
    }
}
