using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class OwnerProfile
{
    public int UserId { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public virtual ICollection<Horse> Horses { get; set; } = new List<Horse>();

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual User User { get; set; } = null!;
}
