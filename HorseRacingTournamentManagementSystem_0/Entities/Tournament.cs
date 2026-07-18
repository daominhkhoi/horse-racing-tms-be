using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class Tournament
{
    public int TourId { get; set; }

    public string TourName { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Location { get; set; }

    public decimal? PrizePool { get; set; }

    public string? Status { get; set; }

    public bool IsHidden { get; set; }

    public string? BannerUrl { get; set; }

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();

    public virtual ICollection<Race> Races { get; set; } = new List<Race>();
}
