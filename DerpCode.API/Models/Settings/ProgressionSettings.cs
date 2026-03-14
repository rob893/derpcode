namespace DerpCode.API.Models.Settings;

public sealed record ProgressionSettings
{
    public int VeryEasyMaxXp { get; init; } = 50;

    public int EasyMaxXp { get; init; } = 75;

    public int MediumMaxXp { get; init; } = 110;

    public int HardMaxXp { get; init; } = 160;

    public int VeryHardMaxXp { get; init; } = 230;

    public int VeryEasyTargetMinutes { get; init; } = 5;

    public int EasyTargetMinutes { get; init; } = 10;

    public int MediumTargetMinutes { get; init; } = 20;

    public int HardTargetMinutes { get; init; } = 35;

    public int VeryHardTargetMinutes { get; init; } = 55;

    public decimal AttemptPenaltyPerExtraAttempt { get; init; } = 0.10m;

    public decimal MinAttemptFactor { get; init; } = 0.60m;

    public decimal HintPenaltyPerHint { get; init; } = 0.12m;

    public decimal MinHintFactor { get; init; } = 0.50m;

    public decimal MinTimeFactor { get; init; } = 0.60m;

    public int LevelBaseXp { get; init; } = 100;
}
