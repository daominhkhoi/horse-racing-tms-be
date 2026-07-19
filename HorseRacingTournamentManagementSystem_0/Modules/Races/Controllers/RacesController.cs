using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;
using HorseRacingTournamentManagementSystem_0.Database;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RacesController : ControllerBase
    {
        private readonly IRaceService _raceService;
        private readonly HorseRacingDbContext _context;

        public RacesController(IRaceService raceService, HorseRacingDbContext context)
        {
            _raceService = raceService;
            _context = context;
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Referee")]
        public async Task<IActionResult> UpdateRaceStatus(int id, [FromBody] string status)
        {
            try
            {
                if (status != "Completed")
                    return BadRequest(new { message = "Referees can only mark a race as completed through this endpoint." });

                var userIdValue = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdValue, out var refereeId))
                    return Unauthorized();

                var isAssigned = await _context.RefereeAssignments
                    .AnyAsync(a => a.RaceId == id && a.RefereeId == refereeId);
                if (!isAssigned)
                    return Forbid();

                var currentStatus = await _context.Races
                    .Where(r => r.RaceId == id)
                    .Select(r => r.Status)
                    .FirstOrDefaultAsync();
                if (currentStatus == null)
                    return NotFound(new { message = "Race not found" });
                if (currentStatus != "Racing" && currentStatus != "Started" && currentStatus != "Live" && currentStatus != "LIVE" && currentStatus != "Ongoing")
                    return Conflict(new { message = "Only a race in progress can be marked as completed." });

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
        [Authorize(Roles = "Referee")]
        public async Task<IActionResult> SubmitResults(int id, [FromBody] SubmitRaceResultDto dto)
        {
            try
            {
                var userIdValue = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdValue, out var userId)) return Unauthorized();
                var isAssigned = await _context.RefereeAssignments.AnyAsync(a => a.RaceId == id && a.RefereeId == userId);
                if (!isAssigned) return Forbid();

                var success = await _raceService.SubmitRaceResultsAsync(id, dto);
                if (!success)
                    return NotFound(new { message = "Race not found" });

                return Ok(new { message = "Race results submitted successfully!" });
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Referee")]
        public async Task<IActionResult> GetRacesForReferee()
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user ID" });
                }
                
                var races = await _raceService.GetRacesForRefereeAsync(userId, false);
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
        [Authorize(Roles = "Referee")]
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

                var isAssigned = await _context.RefereeAssignments
                    .AnyAsync(a => a.RaceId == id && a.RefereeId == refereeId);
                if (!isAssigned)
                {
                    return Forbid();
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

        [HttpGet("{id}/incidents")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetIncidents(int id)
        {
            var raceExists = await _context.Races.AnyAsync(r => r.RaceId == id);
            if (!raceExists)
                return NotFound(new { message = "Race not found" });

            var incidents = await _raceService.GetRaceIncidentsAsync(id);
            return Ok(incidents);
        }
    }
}
