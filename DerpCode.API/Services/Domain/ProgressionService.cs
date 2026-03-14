using System;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Settings;
using DerpCode.API.Utilities;
using Microsoft.Extensions.Options;

namespace DerpCode.API.Services.Domain;

public sealed class ProgressionService : IProgressionService
{
    private readonly ProgressionSettings settings;

    public ProgressionService(IOptions<ProgressionSettings> settings)
    {
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public int CalculateEarnedXp(ProblemDifficulty difficulty, int attemptsBeforePass, TimeSpan solveDuration, int uniqueHintsOpened)
    {
        var maxXp = this.GetMaxXpForDifficulty(difficulty);
        var attempts = Math.Max(attemptsBeforePass, 0);
        var hints = Math.Max(uniqueHintsOpened, 0);
        var totalMinutes = Math.Max(solveDuration.TotalMinutes, 1);
        var targetMinutes = this.GetTargetMinutesForDifficulty(difficulty);

        var attemptFactor = Math.Max(this.settings.MinAttemptFactor, 1 - (this.settings.AttemptPenaltyPerExtraAttempt * attempts));
        var rawTimeFactor = (decimal)(targetMinutes / totalMinutes);
        var timeFactor = Math.Max(this.settings.MinTimeFactor, Math.Min(1m, rawTimeFactor));
        var hintFactor = Math.Max(this.settings.MinHintFactor, 1 - (this.settings.HintPenaltyPerHint * hints));

        var score = (decimal)maxXp * attemptFactor * timeFactor * hintFactor;
        return Math.Max(0, Convert.ToInt32(Math.Round(score, MidpointRounding.AwayFromZero)));
    }

    public int GetMaxXpForDifficulty(ProblemDifficulty difficulty)
    {
        return difficulty switch
        {
            ProblemDifficulty.VeryEasy => this.settings.VeryEasyMaxXp,
            ProblemDifficulty.Easy => this.settings.EasyMaxXp,
            ProblemDifficulty.Medium => this.settings.MediumMaxXp,
            ProblemDifficulty.Hard => this.settings.HardMaxXp,
            ProblemDifficulty.VeryHard => this.settings.VeryHardMaxXp,
            _ => this.settings.EasyMaxXp
        };
    }

    public int GetCycleIndex(DateTimeOffset anchor, DateTimeOffset at)
    {
        return ProgressionMath.GetAnchoredMonthlyCycleIndex(anchor, at);
    }

    public LevelProgress GetLevelProgress(int totalXp)
    {
        return ProgressionMath.GetLevelProgress(totalXp, this.settings.LevelBaseXp);
    }

    private int GetTargetMinutesForDifficulty(ProblemDifficulty difficulty)
    {
        return difficulty switch
        {
            ProblemDifficulty.VeryEasy => this.settings.VeryEasyTargetMinutes,
            ProblemDifficulty.Easy => this.settings.EasyTargetMinutes,
            ProblemDifficulty.Medium => this.settings.MediumTargetMinutes,
            ProblemDifficulty.Hard => this.settings.HardTargetMinutes,
            ProblemDifficulty.VeryHard => this.settings.VeryHardTargetMinutes,
            _ => this.settings.EasyTargetMinutes
        };
    }
}
