using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class Horse
{
    public int HorseId { get; set; }

    public int OwnerId { get; set; }

    public string HorseName { get; set; } = null!;

    public string? Breed { get; set; }

    public int? Age { get; set; }

    public double? Weight { get; set; }

    public string? Gender { get; set; }

    public string? HealthStatus { get; set; }

    public string? ImageUrl { get; set; }

    public string? Status { get; set; } = "Pending";

    [NotMapped]
    public string? OwnerName => Owner?.User?.FullName;

    public virtual ICollection<HorseVerification> HorseVerifications { get; set; } = new List<HorseVerification>();

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual OwnerProfile Owner { get; set; } = null!;

    public virtual ICollection<RaceParticipant> RaceParticipants { get; set; } = new List<RaceParticipant>();
}
