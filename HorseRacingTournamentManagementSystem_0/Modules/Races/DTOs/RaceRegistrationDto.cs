using System;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs
{
    public class RegisterHorseRequest
    {
        public int HorseId { get; set; }
    }

    public class RejectRegistrationRequest
    {
        public string? Note { get; set; }
    }

    public class RaceRegistrationDto
    {
        public int RegistrationId { get; set; }
        public int RaceId { get; set; }
        public string? RaceName { get; set; }
        public int TourId { get; set; }
        public int HorseId { get; set; }

        public string? HorseName { get; set; }
        public string? HorseImageUrl { get; set; }
        public int OwnerId { get; set; }
        public string? OwnerName { get; set; }
        /// <summary>Pending | Approved | Rejected</summary>
        public string Status { get; set; } = "Pending";
        public DateTime RegisteredAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }
    }

    public class RaceStatusDto
    {
        public int RaceId { get; set; }
        public string? RaceName { get; set; }
        public string? Status { get; set; }
        public DateTime? RaceDateTime { get; set; }
        public int? MinParticipants { get; set; }
        public int? MaxParticipants { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
        public string? CancelReason { get; set; }
        public double? Distance { get; set; }
        public double? RewardRatio { get; set; }
    }

    public class ApprovedParticipantDto
    {
        public int ParticipantId { get; set; }
        public int HorseId { get; set; }
        public string? HorseName { get; set; }
        public string? HorseImageUrl { get; set; }
        public int? JockeyId { get; set; }
        public string? JockeyName { get; set; }
        public string? JockeyAvatar { get; set; }
        public int? LaneNumber { get; set; }
        public string? ParticipationStatus { get; set; }
        public int OwnerId { get; set; }
        public string? OwnerName { get; set; }
    }
}
