using System;
using System.Collections.Generic;

namespace DerpCode.API.Models.Entities;

public sealed class UserProblemProgress : IIdentifiable<string>
{
    public string Id => $"{this.UserId}:{this.ProblemId}";

    public int UserId { get; set; }

    public User User { get; set; } = default!;

    public int ProblemId { get; set; }

    public Problem Problem { get; set; } = default!;

    public int BestXp { get; set; }

    public DateTimeOffset? FirstXpAwardedAt { get; set; }

    public int LastAwardedCycleIndex { get; set; } = -1;

    public DateTimeOffset? FirstSubmitAtCurrentCycle { get; set; }

    public int SubmitAttemptsCurrentCycle { get; set; }

    public List<int> OpenedHintIndicesCurrentCycle { get; set; } = [];

    public DateTimeOffset? LastSolvedAt { get; set; }
}
