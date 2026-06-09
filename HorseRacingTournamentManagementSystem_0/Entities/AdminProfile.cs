using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class AdminProfile
{
    public int UserId { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public virtual User User { get; set; } = null!;
}
