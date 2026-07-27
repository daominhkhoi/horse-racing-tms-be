using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.Controllers
{
    [Route("api/jockeys")]
    [ApiController]
    public class JockeysController : ControllerBase
    {
        private readonly IJockeyService _jockeyService;

        public JockeysController(IJockeyService jockeyService)
        {
            _jockeyService = jockeyService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllJockeys()
        {
            try
            {
                var jockeys = await _jockeyService.GetAllJockeysPublicAsync();
                return Ok(new
                {
                    message = "Jockey list retrieved successfully!",
                    data = jockeys
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error retrieving jockey list",
                    error = ex.Message
                });
            }
        }

        [HttpGet("available/race/{raceId}")]
        [Authorize(Roles = "HorseOwner")]
        public async Task<IActionResult> GetAvailableJockeysForRace(int raceId)
        {
            try
            {
                var jockeys = await _jockeyService.GetAvailableJockeysForRaceAsync(raceId);
                return Ok(new
                {
                    message = "Available jockeys retrieved successfully!",
                    data = jockeys
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error retrieving available jockeys",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Jockey")]
        public async Task<IActionResult> RequestUpdateProfile(int id, [FromBody] UpdateJockeyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _jockeyService.RequestUpdateProfileAsync(id, dto);

                if (!result)
                    return NotFound(new { message = "Jockey profile not found for this ID." });

                return Ok(new
                {
                    message = "Update request submitted successfully! Please wait for Admin approval."
                });
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("Phone number"))
                {
                    return BadRequest(new { message = ex.Message });
                }
                return StatusCode(500, new
                {
                    message = "Error submitting jockey update request",
                    error = ex.Message
                });
            }
        }

        [HttpPut("{id}/review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewUpdateRequest(int id, [FromBody] ReviewJockeyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _jockeyService.ReviewUpdateRequestAsync(id, dto);

                if (!result)
                    return NotFound(new
                    {
                        message = "No pending update request found for this jockey."
                    });

                var action = dto.IsApproved ? "approved" : "rejected";
                return Ok(new
                {
                    message = $"Jockey update request {action} successfully!"
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error processing jockey update review",
                    error = ex.Message
                });
            }
        }
    }
}
