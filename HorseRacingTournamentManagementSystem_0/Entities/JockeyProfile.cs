using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class JockeyProfile
{
    public int UserId { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public double? Weight { get; set; }

    public int? ExperienceYear { get; set; }

    public int? ExpYears { get; set; }

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();

    public virtual ICollection<RaceParticipant> RaceParticipants { get; set; } = new List<RaceParticipant>();

    public virtual User User { get; set; } = null!;
}
