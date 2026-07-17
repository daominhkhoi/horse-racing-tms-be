using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitations.DTOs
{
    public class SendInvitationRequest
    {
        [Required]
        public int JockeyId { get; set; }

        [Required]
        public int HorseId { get; set; }

        [Required]
        public int TourId { get; set; }

        public int? RaceId { get; set; }

        public string? Message { get; set; }
    }
}
