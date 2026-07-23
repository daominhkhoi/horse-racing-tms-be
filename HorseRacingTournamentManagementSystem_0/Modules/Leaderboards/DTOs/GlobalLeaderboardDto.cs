using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Modules.Leaderboards.DTOs;

public class GlobalLeaderboardDto
{
    public int HorseId { get; set; }
    public string Horse { get; set; } = null!;
    public string Jockey { get; set; } = null!;
    public int Wins { get; set; }
    public double Points { get; set; }
    public double WinRate { get; set; }
    public string Breed { get; set; } = null!;
    public List<string> Form { get; set; } = new();
    public string? ImageUrl { get; set; }
    public string AvatarBg { get; set; } = "from-sky-400 to-blue-600";
    public string TextCol { get; set; } = "text-blue-500";
}
