using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class RefereeProfile
{
    public int UserId { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public int? ExpYears { get; set; }

    public virtual ICollection<HorseVerification> HorseVerifications { get; set; } = new List<HorseVerification>();

    public virtual ICollection<RefereeAssignment> RefereeAssignments { get; set; } = new List<RefereeAssignment>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
