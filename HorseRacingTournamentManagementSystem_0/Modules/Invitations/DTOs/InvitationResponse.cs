using System;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitations.DTOs
{
    public class InvitationResponse
    {
        public int InviteId { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = null!;
        public int JockeyId { get; set; }
        public string JockeyName { get; set; } = null!;
        public int HorseId { get; set; }
        public string HorseName { get; set; } = null!;
        public int TourId { get; set; }
        public string TourName { get; set; } = null!;
        public string? Message { get; set; }
        public string? Status { get; set; }
        public DateTime? SentAt { get; set; }
        public int? RaceId { get; set; }
        public string? RaceName { get; set; }
    }
}
