using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class SpectatorProfile
{
    public int UserId { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public double? TotalPoints { get; set; }

    public virtual ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();

    public virtual ICollection<RewardTransaction> RewardTransactions { get; set; } = new List<RewardTransaction>();

    public virtual ICollection<TopupTransaction> TopupTransactions { get; set; } = new List<TopupTransaction>();

    public virtual User User { get; set; } = null!;
}
