using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class Leaderboard
{
    public int BoardId { get; set; }

    public int TourId { get; set; }

    public int HorseId { get; set; }

    public int JockeyId { get; set; }

    public double? TotalPoints { get; set; }

    public int? TotalWins { get; set; }

    public int? Rank { get; set; }

    public virtual Horse Horse { get; set; } = null!;

    public virtual JockeyProfile Jockey { get; set; } = null!;

    public virtual Tournament Tour { get; set; } = null!;
}
