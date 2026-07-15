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
    }
}
