using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class RaceParticipant
{
    public int ParticipantId { get; set; }

    public int RaceId { get; set; }

    public int HorseId { get; set; }

    public int JockeyId { get; set; }

    public int? LaneNumber { get; set; }

    public string? CheckInStatus { get; set; }

    public string? ParticipationStatus { get; set; }

    public virtual Horse Horse { get; set; } = null!;

    public virtual JockeyProfile Jockey { get; set; } = null!;

    public virtual ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();

    public virtual Race Race { get; set; } = null!;

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
