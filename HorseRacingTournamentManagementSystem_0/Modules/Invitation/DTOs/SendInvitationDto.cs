using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitation.DTOs
{
    /// <summary>
    /// DTO for FR-INVT-001: HorseOwner sends an invitation to a Jockey for a specific horse.
    ///
    /// Endpoint: POST /api/invitations
    /// </summary>
    public class SendInvitationDto
    {
        /// <summary>The ID of the Jockey being invited.</summary>
        [Required(ErrorMessage = "JockeyId is required")]
        public int JockeyId { get; set; }

        /// <summary>The horse the owner wants this jockey to ride.</summary>
        [Required(ErrorMessage = "HorseId is required")]
        public int HorseId { get; set; }

        /// <summary>The tournament context for this invitation.</summary>
        [Required(ErrorMessage = "TourId is required")]
        public int TourId { get; set; }

        /// <summary>Optional message from the owner to the jockey.</summary>
        [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string? Message { get; set; }
    }
}
