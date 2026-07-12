using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Invitation.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitation.Interfaces
{
    /// <summary>
    /// Interface defining the contract for InvitationService.
    ///
    /// Feature coverage:
    ///   FR-INVT-001 → SendInvitationAsync         (HorseOwner sends invitation)
    ///   FR-INVT-002 → GetSentInvitationsAsync      (HorseOwner views sent invitations)
    ///   FR-INVT-003 → GetReceivedInvitationsAsync  (Jockey views received invitations)
    ///   FR-INVT-004 → RespondToInvitationAsync     (Jockey accepts/rejects)
    ///   FR-INVT-005 → Auto-cancel logic embedded inside RespondToInvitationAsync
    /// </summary>
    public interface IInvitationService
    {
        /// <summary>
        /// [FR-INVT-001] HorseOwner sends an invitation to a Jockey for a specific horse.
        ///
        /// Rules:
        ///   - Horse must belong to the owner.
        ///   - No existing active (Pending) invitation for the same Horse + Jockey + Tour combination.
        ///   - Default status = "Pending".
        /// </summary>
        /// <param name="ownerId">UserId of the HorseOwner sending the invitation.</param>
        /// <param name="dto">Invitation details (JockeyId, HorseId, TourId, Message).</param>
        /// <returns>Tuple: Success flag + descriptive message (for error cases).</returns>
        Task<(bool Success, string Message)> SendInvitationAsync(int ownerId, SendInvitationDto dto);

        /// <summary>
        /// [FR-INVT-002] HorseOwner views all invitations they have sent.
        ///
        /// Supports optional filtering by Status (Pending, Accepted, Rejected, Cancelled, AutoCancelled).
        /// </summary>
        /// <param name="ownerId">UserId of the HorseOwner.</param>
        /// <param name="status">Optional status filter; null returns all.</param>
        /// <returns>List of InvitationDto ordered newest-first.</returns>
        Task<IEnumerable<InvitationDto>> GetSentInvitationsAsync(int ownerId, string? status);

        /// <summary>
        /// [FR-INVT-003] Jockey views all invitations they have received.
        /// </summary>
        /// <param name="jockeyId">UserId of the Jockey.</param>
        /// <returns>List of InvitationDto ordered newest-first.</returns>
        Task<IEnumerable<InvitationDto>> GetReceivedInvitationsAsync(int jockeyId);

        /// <summary>
        /// [FR-INVT-004 + FR-INVT-005] Jockey accepts or rejects an invitation.
        ///
        /// Rules:
        ///   - Invitation must belong to this jockey (ownership check).
        ///   - Invitation must be in "Pending" status (prevent responding multiple times).
        ///   - If accepted: auto-cancel ALL other Pending invitations for the same Horse (FR-INVT-005).
        ///   - All DB writes execute within a single transaction to prevent race conditions.
        /// </summary>
        /// <param name="jockeyId">UserId of the Jockey responding.</param>
        /// <param name="inviteId">ID of the invitation to respond to.</param>
        /// <param name="accept">true = Accept, false = Reject.</param>
        /// <returns>Tuple: Success flag + descriptive message.</returns>
        Task<(bool Success, string Message)> RespondToInvitationAsync(int jockeyId, int inviteId, bool accept);

        /// <summary>
        /// [FR-INVT-006] HorseOwner cancels a pending invitation they sent.
        ///
        /// Rules:
        ///   - Invitation must belong to the owner.
        ///   - Invitation must be in "Pending" status.
        /// </summary>
        /// <param name="ownerId">UserId of the HorseOwner cancelling.</param>
        /// <param name="inviteId">ID of the invitation to cancel.</param>
        /// <returns>Tuple: Success flag + descriptive message.</returns>
        Task<(bool Success, string Message)> CancelInvitationAsync(int ownerId, int inviteId);
    }
}
