using System;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class RaceComment
{
    public int Id { get; set; }

    public int RaceId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Race Race { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
