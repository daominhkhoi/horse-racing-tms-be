using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class RaceRegistration
{
    public int RegistrationId { get; set; }

    public int RaceId { get; set; }

    public int HorseId { get; set; }

    public int OwnerId { get; set; }

    /// <summary>Pending | Approved | Rejected</summary>
    public string Status { get; set; } = "Pending";

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewNote { get; set; }

    // Navigation
    public virtual Race Race { get; set; } = null!;
    public virtual Horse Horse { get; set; } = null!;
    public virtual OwnerProfile Owner { get; set; } = null!;
}
