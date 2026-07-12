using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitation.DTOs
{
    /// <summary>
    /// DTO for FR-INVT-004: Jockey accepts or rejects an invitation.
    ///
    /// Endpoint: PUT /api/invitations/{id}/respond
    /// </summary>
    public class RespondInvitationDto
    {
        /// <summary>
        /// true  → Jockey ACCEPTS the invitation.
        /// false → Jockey REJECTS the invitation.
        /// </summary>
        [Required(ErrorMessage = "Accept field is required (true = accept / false = reject)")]
        public bool Accept { get; set; }
    }
}
