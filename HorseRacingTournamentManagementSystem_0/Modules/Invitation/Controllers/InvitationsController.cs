using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Invitation.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Invitation.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitation.Controllers
{
    /// <summary>
    /// Controller handling HTTP requests for the Invitation feature.
    ///
    /// Base route: /api/invitations
    ///
    /// Endpoints:
    ///   POST /api/invitations                   → FR-INVT-001: HorseOwner sends invitation
    ///   GET  /api/invitations/sent              → FR-INVT-002: HorseOwner views sent invitations
    ///   GET  /api/invitations/my                → FR-INVT-003: Jockey views received invitations
    ///   PUT  /api/invitations/{id}/respond      → FR-INVT-004 + FR-INVT-005: Jockey responds
    /// </summary>
    [Route("api/invitations")]
    [ApiController]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationService _invitationService;

        public InvitationsController(IInvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        // =====================================================================
        // FR-INVT-001: POST /api/invitations
        // HorseOwner sends invitation to a Jockey for a specific horse
        // =====================================================================

        /// <summary>
        /// [FR-INVT-001] HorseOwner sends a new invitation.
        ///
        /// FLOW:
        ///   Client → POST /api/invitations  Body: { jockeyId, horseId, tourId, message? }
        ///   Controller → reads OwnerId from JWT claims
        ///   Controller → calls _invitationService.SendInvitationAsync(ownerId, dto)
        ///   Service    → validates, checks duplicates, creates invitation with Status="Pending"
        ///   Controller → returns 200 OK / 400 BadRequest
        ///
        /// AUTHORIZATION: [Authorize(Roles = "HorseOwner")]
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "HorseOwner")]
        public async Task<IActionResult> SendInvitation([FromBody] SendInvitationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extract owner's UserId from JWT
            var ownerIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(ownerIdStr, out int ownerId))
                return Unauthorized(new { message = "Invalid token: cannot identify user." });

            try
            {
                var (success, message) = await _invitationService.SendInvitationAsync(ownerId, dto);

                if (!success)
                    return BadRequest(new { message });

                return Ok(new { message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error sending invitation",
                    error = ex.Message
                });
            }
        }

        // =====================================================================
        // FR-INVT-002: GET /api/invitations/sent
        // HorseOwner views all invitations they have sent (filter by status)
        // =====================================================================

        /// <summary>
        /// [FR-INVT-002] HorseOwner retrieves all invitations they have sent.
        ///
        /// FLOW:
        ///   Client → GET /api/invitations/sent?status=Pending  (status optional)
        ///   Controller → reads OwnerId from JWT claims
        ///   Controller → calls _invitationService.GetSentInvitationsAsync(ownerId, status)
        ///   Service    → queries DB with optional filter, returns flat DTOs
        ///   Controller → returns 200 OK
        ///
        /// AUTHORIZATION: [Authorize(Roles = "HorseOwner")]
        /// </summary>
        [HttpGet("sent")]
        [Authorize(Roles = "HorseOwner")]
        public async Task<IActionResult> GetSentInvitations([FromQuery] string? status = null)
        {
            var ownerIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(ownerIdStr, out int ownerId))
                return Unauthorized(new { message = "Invalid token: cannot identify user." });

            try
            {
                var invitations = await _invitationService.GetSentInvitationsAsync(ownerId, status);
                return Ok(new
                {
                    message = "Sent invitations retrieved successfully!",
                    data = invitations
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error retrieving sent invitations",
                    error = ex.Message
                });
            }
        }

        // =====================================================================
        // FR-INVT-003: GET /api/invitations/my
        // Jockey views all invitations they have received
        // =====================================================================

        /// <summary>
        /// [FR-INVT-003] Jockey retrieves all invitations they have received.
        ///
        /// FLOW:
        ///   Client → GET /api/invitations/my
        ///   Controller → reads JockeyId from JWT claims
        ///   Controller → calls _invitationService.GetReceivedInvitationsAsync(jockeyId)
        ///   Service    → queries DB, returns flat DTOs ordered newest-first
        ///   Controller → returns 200 OK
        ///
        /// AUTHORIZATION: [Authorize(Roles = "Jockey")]
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Jockey")]
        public async Task<IActionResult> GetMyInvitations()
        {
            var jockeyIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(jockeyIdStr, out int jockeyId))
                return Unauthorized(new { message = "Invalid token: cannot identify user." });

            try
            {
                var invitations = await _invitationService.GetReceivedInvitationsAsync(jockeyId);
                return Ok(new
                {
                    message = "Received invitations retrieved successfully!",
                    data = invitations
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error retrieving received invitations",
                    error = ex.Message
                });
            }
        }

        // =====================================================================
        // FR-INVT-004 + FR-INVT-005: PUT /api/invitations/{id}/respond
        // Jockey accepts or rejects; auto-cancel other pending for same horse
        // =====================================================================

        /// <summary>
        /// [FR-INVT-004 + FR-INVT-005] Jockey responds to an invitation.
        ///
        /// FLOW:
        ///   Client → PUT /api/invitations/{id}/respond  Body: { accept: true/false }
        ///   Controller → reads JockeyId from JWT claims
        ///   Controller → calls _invitationService.RespondToInvitationAsync(jockeyId, id, accept)
        ///   Service    → validates ownership + status, responds, auto-cancels duplicates (FR-INVT-005)
        ///                ALL inside a DB transaction
        ///   Controller → returns 200 OK / 400 BadRequest / 403 Forbidden
        ///
        /// AUTHORIZATION: [Authorize(Roles = "Jockey")]
        /// </summary>
        [HttpPut("{id}/respond")]
        [Authorize(Roles = "Jockey")]
        public async Task<IActionResult> RespondToInvitation(int id, [FromBody] RespondInvitationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var jockeyIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(jockeyIdStr, out int jockeyId))
                return Unauthorized(new { message = "Invalid token: cannot identify user." });

            try
            {
                var (success, message) = await _invitationService.RespondToInvitationAsync(jockeyId, id, dto.Accept);

                if (!success)
                {
                    // Distinguish ownership errors (403) from logic errors (400)
                    if (message.Contains("permission"))
                        return StatusCode(403, new { message });

                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error responding to invitation",
                    error = ex.Message
                });
            }
        }

        // =====================================================================
        // FR-INVT-006: PUT /api/invitations/{id}/cancel
        // HorseOwner cancels their pending invitation
        // =====================================================================

        /// <summary>
        /// [FR-INVT-006] HorseOwner cancels a pending invitation.
        ///
        /// FLOW:
        ///   Client → PUT /api/invitations/{id}/cancel
        ///   Controller → reads OwnerId from JWT claims
        ///   Controller → calls _invitationService.CancelInvitationAsync(ownerId, id)
        ///   Service    → validates ownership + status, cancels invitation
        ///   Controller → returns 200 OK / 400 BadRequest / 403 Forbidden
        ///
        /// AUTHORIZATION: [Authorize(Roles = "HorseOwner")]
        /// </summary>
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "HorseOwner")]
        public async Task<IActionResult> CancelInvitation(int id)
        {
            var ownerIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(ownerIdStr, out int ownerId))
                return Unauthorized(new { message = "Invalid token: cannot identify user." });

            try
             {
                var (success, message) = await _invitationService.CancelInvitationAsync(ownerId, id);

                if (!success)
                {
                    if (message.Contains("permission"))
                        return StatusCode(403, new { message });

                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error cancelling invitation",
                    error = ex.Message
                });
            }
        }
    }
}
