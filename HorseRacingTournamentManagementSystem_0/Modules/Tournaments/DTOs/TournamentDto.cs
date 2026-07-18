using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Modules.Tournaments.DTOs
{
    public class TournamentDto
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
        public int ParticipantCount { get; set; }
    }

    public class TournamentDetailDto : TournamentDto
    {
        public List<RaceDto> Races { get; set; } = new List<RaceDto>();
    }

    public class RaceDto
    {
        public int RaceId { get; set; }
        public string? RaceName { get; set; }
        public int? Round { get; set; }
        public DateTime? RaceDateTime { get; set; }
        public double? Distance { get; set; }
        public double? RewardRatio { get; set; }
        public string? Status { get; set; }
        public int MinParticipants { get; set; }
        public int MaxParticipants { get; set; }
        public string? CancelReason { get; set; }
        
        public List<RaceParticipantDto> Participants { get; set; } = new List<RaceParticipantDto>();
        public List<RefereeAssignmentDto> Referees { get; set; } = new List<RefereeAssignmentDto>();
    }

    public class RaceParticipantDto
    {
        public int ParticipantId { get; set; }
        public int HorseId { get; set; }
        public string? HorseName { get; set; }
        public string? HorseAvatar { get; set; }
        public int JockeyId { get; set; }
        public string? JockeyName { get; set; }
        public string? JockeyAvatar { get; set; }
        public int? LaneNumber { get; set; }
        public string? ParticipationStatus { get; set; }
    }

    public class RefereeAssignmentDto
    {
        public int AssignId { get; set; }
        public int RefereeId { get; set; }
        public string? RefereeName { get; set; }
        public string? RefereeAvatar { get; set; }
    }
}
