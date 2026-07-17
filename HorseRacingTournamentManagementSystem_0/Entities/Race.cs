using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class Race
{
    public int RaceId { get; set; }

    public int TourId { get; set; }

    public string? RaceName { get; set; }

    public int? Round { get; set; }

    public DateTime? RaceDateTime { get; set; }

    public double? Distance { get; set; }

    public string? Status { get; set; }

    public double? RewardRatio { get; set; }

    public int? MinParticipants { get; set; } = 4;

    public int? MaxParticipants { get; set; } = 12;

    public string? CancelReason { get; set; }

    public virtual ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();

    public virtual ICollection<RaceParticipant> RaceParticipants { get; set; } = new List<RaceParticipant>();

    public virtual ICollection<RefereeAssignment> RefereeAssignments { get; set; } = new List<RefereeAssignment>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual Tournament Tour { get; set; } = null!;

    public virtual ICollection<RaceRegistration> RaceRegistrations { get; set; } = new List<RaceRegistration>();

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
