using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class Violation
{
    public int ViolationId { get; set; }

    public int RaceId { get; set; }

    public int ParticipantId { get; set; }

    public int RefereeId { get; set; }

    public string? ViolationType { get; set; }

    public string? Penalty { get; set; }

    public string? Description { get; set; }

    public virtual RaceParticipant Participant { get; set; } = null!;

    public virtual Race Race { get; set; } = null!;

    public virtual RefereeProfile Referee { get; set; } = null!;
}
