using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class RewardTransaction
{
    public int TranId { get; set; }

    public int SpectatorId { get; set; }

    public int PredictionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? TransactionDate { get; set; }

    public virtual Prediction Prediction { get; set; } = null!;

    public virtual SpectatorProfile Spectator { get; set; } = null!;
}
