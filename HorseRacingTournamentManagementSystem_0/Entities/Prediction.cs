using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class Prediction
{
    public int PredictionId { get; set; }

    public int RaceId { get; set; }

    public int SpectatorId { get; set; }

    public int ParticipantId { get; set; }

    public double BetPoints { get; set; }

    public string? Status { get; set; }

    public double? RewardPoints { get; set; }

    public virtual RaceParticipant Participant { get; set; } = null!;

    public virtual Race Race { get; set; } = null!;

    public virtual ICollection<RewardTransaction> RewardTransactions { get; set; } = new List<RewardTransaction>();

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual SpectatorProfile Spectator { get; set; } = null!;
}
