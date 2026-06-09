using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class RefereeAssignment
{
    public int AssignId { get; set; }

    public int RaceId { get; set; }

    public int RefereeId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual Race Race { get; set; } = null!;

    public virtual RefereeProfile Referee { get; set; } = null!;
}
