using DerpCode.API.Utilities;

namespace DerpCode.API.Tests.Utilities;

public sealed class ProgressionMathTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 100)]
    [InlineData(3, 400)]
    [InlineData(4, 900)]
    public void GetXpToReachLevel_WithDefaultBase_ReturnsExpectedThreshold(int level, int expectedXp)
    {
        var xp = ProgressionMath.GetXpToReachLevel(level);

        Assert.Equal(expectedXp, xp);
    }

    [Fact]
    public void GetXpToReachLevel_WithCustomBase_UsesProvidedBase()
    {
        var xp = ProgressionMath.GetXpToReachLevel(4, levelBaseXp: 50);

        Assert.Equal(450, xp);
    }

    [Theory]
    [InlineData(0, 1, 0, 100)]
    [InlineData(99, 1, 99, 100)]
    [InlineData(100, 2, 0, 300)]
    [InlineData(399, 2, 299, 300)]
    [InlineData(400, 3, 0, 500)]
    public void GetLevelProgress_WithDefaultBase_ReturnsExpectedProgress(
        int totalXp,
        int expectedLevel,
        int expectedIntoLevel,
        int expectedForNextLevel)
    {
        var progress = ProgressionMath.GetLevelProgress(totalXp);

        Assert.Equal(expectedLevel, progress.Level);
        Assert.Equal(expectedIntoLevel, progress.XpIntoLevel);
        Assert.Equal(expectedForNextLevel, progress.XpForNextLevel);
    }

    [Fact]
    public void GetLevelProgress_WithNegativeTotalXp_ClampsToZeroProgress()
    {
        var progress = ProgressionMath.GetLevelProgress(-25);

        Assert.Equal(1, progress.Level);
        Assert.Equal(0, progress.XpIntoLevel);
        Assert.Equal(100, progress.XpForNextLevel);
    }

    [Fact]
    public void GetAnchoredMonthlyCycleIndex_WhenAtIsBeforeOrEqualAnchor_ReturnsZero()
    {
        var anchor = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);

        Assert.Equal(0, ProgressionMath.GetAnchoredMonthlyCycleIndex(anchor, anchor));
        Assert.Equal(0, ProgressionMath.GetAnchoredMonthlyCycleIndex(anchor, anchor.AddDays(-1)));
    }

    [Fact]
    public void GetAnchoredMonthlyCycleIndex_WithElapsedMonths_ReturnsExpectedCycle()
    {
        var anchor = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);

        Assert.Equal(0, ProgressionMath.GetAnchoredMonthlyCycleIndex(anchor, anchor.AddMonths(1).AddTicks(-1)));
        Assert.Equal(1, ProgressionMath.GetAnchoredMonthlyCycleIndex(anchor, anchor.AddMonths(1)));
        Assert.Equal(2, ProgressionMath.GetAnchoredMonthlyCycleIndex(anchor, anchor.AddMonths(2).AddDays(10)));
    }

    [Fact]
    public void GetNextCycleStart_ReturnsStartOfFollowingCycle()
    {
        var anchor = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);

        var cycle0Next = ProgressionMath.GetNextCycleStart(anchor, 0);
        var cycle2Next = ProgressionMath.GetNextCycleStart(anchor, 2);

        Assert.Equal(anchor.AddMonths(1), cycle0Next);
        Assert.Equal(anchor.AddMonths(3), cycle2Next);
    }
}
