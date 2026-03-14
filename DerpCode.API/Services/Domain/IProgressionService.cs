using System;
using DerpCode.API.Models.Entities;
using DerpCode.API.Utilities;

namespace DerpCode.API.Services.Domain;

public interface IProgressionService
{
    int CalculateEarnedXp(ProblemDifficulty difficulty, int attemptsBeforePass, TimeSpan solveDuration, int uniqueHintsOpened);

    int GetCycleIndex(DateTimeOffset anchor, DateTimeOffset at);

    LevelProgress GetLevelProgress(int totalXp);

    int GetMaxXpForDifficulty(ProblemDifficulty difficulty);
}
