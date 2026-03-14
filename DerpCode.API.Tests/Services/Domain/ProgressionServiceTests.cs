using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Settings;
using DerpCode.API.Services.Domain;
using Microsoft.Extensions.Options;

namespace DerpCode.API.Tests.Services.Domain;

public sealed class ProgressionServiceTests
{
    private readonly ProgressionService service;

    public ProgressionServiceTests()
    {
        var settings = Options.Create(new ProgressionSettings());
        this.service = new ProgressionService(settings);
    }

    [Fact]
    public void CalculateEarnedXp_WithPerfectSubmission_ReturnsMaxXpForDifficulty()
    {
        var earned = this.service.CalculateEarnedXp(ProblemDifficulty.Hard, 0, TimeSpan.FromMinutes(1), 0);

        Assert.Equal(160, earned);
    }

    [Fact]
    public void CalculateEarnedXp_WithManyAttemptsAndHints_RespectsFloors()
    {
        var earned = this.service.CalculateEarnedXp(ProblemDifficulty.Easy, 20, TimeSpan.FromMinutes(200), 20);

        // easy max(75) * min attempt(0.6) * min time(0.6) * min hint(0.5) = 13.5 => 14
        Assert.Equal(14, earned);
    }

    [Fact]
    public void GetLevelProgress_WithBoundaryXp_ReturnsExpectedLevel()
    {
        var levelProgress = this.service.GetLevelProgress(400);

        Assert.Equal(3, levelProgress.Level);
        Assert.Equal(0, levelProgress.XpIntoLevel);
        Assert.Equal(500, levelProgress.XpForNextLevel);
    }

    [Fact]
    public void GetCycleIndex_WithTwoMonthsElapsed_ReturnsSecondCycle()
    {
        var anchor = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var at = new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero);

        var cycle = this.service.GetCycleIndex(anchor, at);

        Assert.Equal(2, cycle);
    }
}
