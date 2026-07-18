using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RacesController : ControllerBase
    {
        private readonly IRaceService _raceService;

        public RacesController(IRaceService raceService)
        {
            _raceService = raceService;
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Referee")]
        public async Task<IActionResult> UpdateRaceStatus(int id, [FromBody] string status)
        {
            try
            {
                var success = await _raceService.UpdateRaceStatusAsync(id, status);
                if (!success)
                    return NotFound(new { message = "Race not found" });

                return Ok(new { message = "Race status updated successfully!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error updating race status", error = ex.Message });
            }
        }

        [HttpPost("{id}/results")]
        [Authorize(Roles = "Admin,Referee")]
        public async Task<IActionResult> SubmitResults(int id, [FromBody] SubmitRaceResultDto dto)
        {
            try
            {
                var success = await _raceService.SubmitRaceResultsAsync(id, dto);
                if (!success)
                    return NotFound(new { message = "Race not found" });

                return Ok(new { message = "Race results submitted successfully!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error submitting race results", error = ex.Message });
            }
        }

        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetResults(int id)
        {
            try
            {
                var results = await _raceService.GetRaceResultsAsync(id);
                return Ok(new { message = "Race results retrieved successfully", data = results });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving race results", error = ex.Message });
            }
        }

        [HttpPost("{id}/award")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AwardPrizes(int id)
        {
            try
            {
                var result = await _raceService.AwardPrizesAsync(id);
                if (result.Contains("successfully"))
                {
                    return Ok(new { message = result });
                }
                return BadRequest(new { message = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error awarding prizes", error = ex.Message });
            }
        }

        [HttpGet("streams/active")]
        public async Task<IActionResult> GetActiveStreams()
        {
            try
            {
                var streams = await _raceService.GetActiveStreamsAsync();
                return Ok(streams);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error getting active streams", error = ex.Message });
            }
        }

        [HttpPut("{id}/youtube-id")]
        [Authorize(Roles = "Admin,Referee")]
        public async Task<IActionResult> UpdateYoutubeId(int id, [FromBody] string youtubeId)
        {
            try
            {
                var success = await _raceService.UpdateYoutubeIdAsync(id, youtubeId);
                if (!success)
                    return NotFound(new { message = "Race not found" });

                return Ok(new { message = "Youtube ID updated successfully!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error updating Youtube ID", error = ex.Message });
            }
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetRaceComments(int id)
        {
            try
            {
                var comments = await _raceService.GetRaceCommentsAsync(id);
                return Ok(comments);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error getting race comments", error = ex.Message });
            }
        }
        [HttpGet("referee-list")]
        [Authorize(Roles = "Admin,Referee")]
        public async Task<IActionResult> GetRacesForReferee()
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user ID" });
                }
                
                bool isAdmin = User.IsInRole("Admin");

                var races = await _raceService.GetRacesForRefereeAsync(userId, isAdmin);
                return Ok(races);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("API ERROR GetRacesForReferee: " + ex.ToString());
                return StatusCode(500, new { message = "Error getting races for referee", error = ex.Message });
            }
        }

        [HttpGet("{id}/participants")]
        [Authorize(Roles = "Admin,Referee")]
        public async Task<IActionResult> GetRaceParticipants(int id)
        {
            try
            {
                var participants = await _raceService.GetRaceParticipantsAsync(id);
                return Ok(participants);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error getting race participants", error = ex.Message });
            }
        }

        [HttpPost("{id}/incident")]
        [Authorize(Roles = "Admin,Referee")]
        public async Task<IActionResult> ReportIncident(int id, [FromBody] HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs.CreateViolationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int refereeId))
                {
                    return Unauthorized(new { message = "Invalid referee ID" });
                }

                var success = await _raceService.ReportIncidentAsync(id, refereeId, dto);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to create incident report. Race may not exist." });
                }

                return Ok(new { message = "Incident reported successfully." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error reporting incident", error = ex.Message });
            }
        }
    }
}
