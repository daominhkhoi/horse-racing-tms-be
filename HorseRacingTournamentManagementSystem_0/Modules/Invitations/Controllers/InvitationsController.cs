using HorseRacingTournamentManagementSystem_0.Modules.Invitations.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Invitations.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitations.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationService _invitationService;

        public InvitationsController(IInvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            throw new Exception("User ID not found in token.");
        }

        [HttpPost]
        [Authorize(Roles = "HorseOwner")]
        public async Task<IActionResult> SendInvitation([FromBody] SendInvitationRequest request)
        {
            try
            {
                var ownerId = GetCurrentUserId();
                var result = await _invitationService.SendInvitationAsync(ownerId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "HorseOwner")]
        public async Task<IActionResult> CancelInvitation(int id)
        {
            try
            {
                var ownerId = GetCurrentUserId();
                var result = await _invitationService.CancelInvitationAsync(ownerId, id);
                return Ok(new { success = result, message = "Invitation canceled successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my")]
        [Authorize(Roles = "Jockey")]
        public async Task<IActionResult> GetMyInvitations()
        {
            try
            {
                var jockeyId = GetCurrentUserId();
                var result = await _invitationService.GetMyInvitationsAsync(jockeyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("sent")]
        [Authorize(Roles = "HorseOwner")]
        public async Task<IActionResult> GetSentInvitations()
        {
            try
            {
                var ownerId = GetCurrentUserId();
                var result = await _invitationService.GetSentInvitationsAsync(ownerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/respond")]
        [Authorize(Roles = "Jockey")]
        public async Task<IActionResult> RespondToInvitation(int id, [FromBody] RespondInvitationRequest request)
        {
            try
            {
                var jockeyId = GetCurrentUserId();
                var result = await _invitationService.RespondToInvitationAsync(jockeyId, id, request.IsAccepted);
                return Ok(new { success = result, message = request.IsAccepted ? "Invitation accepted." : "Invitation rejected." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class RespondInvitationRequest
    {
        public bool IsAccepted { get; set; }
    }
}
