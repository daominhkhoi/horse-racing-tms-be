using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RaceRegistrationController : ControllerBase
    {
        private readonly IRaceRegistrationService _raceRegistrationService;

        public RaceRegistrationController(IRaceRegistrationService raceRegistrationService)
        {
            _raceRegistrationService = raceRegistrationService;
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                return userId;
            }
            throw new Exception("User ID not found in token.");
        }

        /// <summary>Horse Owner đăng ký Horse vào Race</summary>
        [HttpPost("~/api/races/{raceId}/register")]
        [Authorize(Roles = "HorseOwner")]
        public async Task<IActionResult> RegisterHorse(int raceId, [FromBody] RegisterHorseRequest request)
        {
            try
            {
                var ownerId = GetCurrentUserId();
                var result = await _raceRegistrationService.RegisterHorseAsync(ownerId, raceId, request.HorseId);
                return Ok(new { message = "Horse registered successfully!", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Admin lấy danh sách đăng ký của một Race</summary>
        [HttpGet("~/api/races/{raceId}/registrations")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRegistrations(int raceId)
        {
            try
            {
                var registrations = await _raceRegistrationService.GetRegistrationsByRaceAsync(raceId);
                return Ok(new { message = "Registrations retrieved successfully", data = registrations });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving registrations", error = ex.Message });
            }
        }

        /// <summary>Admin duyệt đăng ký</summary>
        [HttpPut("~/api/races/{raceId}/registrations/{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRegistration(int raceId, int id)
        {
            try
            {
                var result = await _raceRegistrationService.ApproveRegistrationAsync(id);
                return Ok(new { message = "Registration approved successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Admin từ chối đăng ký</summary>
        [HttpPut("~/api/races/{raceId}/registrations/{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRegistration(int raceId, int id, [FromBody] RejectRegistrationRequest request)
        {
            try
            {
                var result = await _raceRegistrationService.RejectRegistrationAsync(id, request?.Note);
                return Ok(new { message = "Registration rejected." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Admin mở đăng ký cho Race</summary>
        [HttpPut("~/api/races/{raceId}/open-registration")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OpenRegistration(int raceId)
        {
            try
            {
                await _raceRegistrationService.OpenRegistrationAsync(raceId);
                return Ok(new { message = "Registration opened successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Admin đóng đăng ký cho Race</summary>
        [HttpPut("~/api/races/{raceId}/close-registration")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CloseRegistration(int raceId)
        {
            try
            {
                await _raceRegistrationService.CloseRegistrationAsync(raceId);
                return Ok(new { message = "Registration closed successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Lấy trạng thái chi tiết của Race</summary>
        [HttpGet("~/api/races/{raceId}/status")]
        public async Task<IActionResult> GetRaceStatus(int raceId)
        {
            try
            {
                var status = await _raceRegistrationService.GetRaceStatusAsync(raceId);
                return Ok(new { message = "Race status retrieved successfully", data = status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving race status", error = ex.Message });
            }
        }

        /// <summary>Lấy danh sách participants đã approved của Race</summary>
        [HttpGet("~/api/races/{raceId}/participants")]
        public async Task<IActionResult> GetParticipants(int raceId)
        {
            try
            {
                var participants = await _raceRegistrationService.GetApprovedParticipantsAsync(raceId);
                return Ok(new { message = "Participants retrieved successfully", data = participants });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving participants", error = ex.Message });
            }
        }

        /// <summary>Horse Owner xem lịch sử đăng ký của mình</summary>
        [HttpGet("~/api/races/my-registrations")]
        [Authorize(Roles = "HorseOwner")]
        public async Task<IActionResult> GetMyRegistrations()
        {
            try
            {
                var ownerId = GetCurrentUserId();
                var registrations = await _raceRegistrationService.GetMyRegistrationsAsync(ownerId);
                return Ok(new { message = "Registrations retrieved successfully", data = registrations });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving registrations", error = ex.Message });
            }
        }
        /// <summary>Admin duyệt Jockey của một Participant</summary>
        [HttpPut("~/api/races/participants/{participantId}/approve-jockey")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveJockey(int participantId)
        {
            try
            {
                var result = await _raceRegistrationService.ApproveJockeyAsync(participantId);
                return Ok(new { message = "Jockey assignment approved successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

