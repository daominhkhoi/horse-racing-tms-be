using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class Result
{
    public int ResultId { get; set; }

    public int RaceId { get; set; }

    public int ParticipantId { get; set; }

    public TimeOnly? FinishTime { get; set; }

    public int? RankPosition { get; set; }

    public decimal? RewardMoney { get; set; }

    public string? ResultStatus { get; set; }

    public virtual RaceParticipant Participant { get; set; } = null!;

    public virtual Race Race { get; set; } = null!;
}
