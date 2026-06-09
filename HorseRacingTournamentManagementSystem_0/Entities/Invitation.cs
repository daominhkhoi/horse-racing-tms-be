using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class Invitation
{
    public int InviteId { get; set; }

    public int OwnerId { get; set; }

    public int JockeyId { get; set; }

    public int HorseId { get; set; }

    public int TourId { get; set; }

    public string? Message { get; set; }

    public string? Status { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual Horse Horse { get; set; } = null!;

    public virtual JockeyProfile Jockey { get; set; } = null!;

    public virtual OwnerProfile Owner { get; set; } = null!;

    public virtual Tournament Tour { get; set; } = null!;
}
