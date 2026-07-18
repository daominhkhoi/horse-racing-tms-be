using System;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs
{
    public class RefereeRaceDto
    {
        public int RaceId { get; set; }
        public string RaceName { get; set; } = string.Empty;
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public string? TournamentBanner { get; set; }
        public string Track { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int HorsesCount { get; set; }
        public string Laps { get; set; } = string.Empty;
        public string Leader { get; set; } = string.Empty;
        public int IncidentsCount { get; set; }
    }
}
