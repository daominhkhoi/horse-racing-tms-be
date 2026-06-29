using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Modules.Tournaments.DTOs
{
    public class CreateTournamentDto
    {
        public string TourName { get; set; } = null!;
        public string? Location { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal? PrizePool { get; set; }
        public List<CreateRaceDto> Races { get; set; } = new List<CreateRaceDto>();
    }

    public class CreateRaceDto
    {
        public string? RaceName { get; set; }
        public DateTime? RaceDateTime { get; set; }
        public double? Distance { get; set; }
        public List<CreateRaceParticipantDto> Participants { get; set; } = new List<CreateRaceParticipantDto>();
    }

    public class CreateRaceParticipantDto
    {
        public int? LaneNumber { get; set; }
        public int HorseId { get; set; }
        public int JockeyId { get; set; }
    }
}
