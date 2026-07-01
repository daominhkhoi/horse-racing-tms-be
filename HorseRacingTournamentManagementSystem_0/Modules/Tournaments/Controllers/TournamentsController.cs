using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Tournaments.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly ITournamentService _tournamentService;

        public TournamentsController(ITournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTournaments([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                var result = await _tournamentService.GetAllTournamentsAsync(page, pageSize, search);
                return Ok(new { message = "Tournaments retrieved successfully", data = result });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving tournaments", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTournamentById(int id)
        {
            try
            {
                var tournament = await _tournamentService.GetTournamentByIdAsync(id);
                return Ok(new { message = "Tournament retrieved successfully", data = tournament });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving tournament", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var tournament = await _tournamentService.CreateTournamentAsync(dto);
                return Ok(new { message = "Tournament created successfully!", data = tournament });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error creating tournament", error = ex.Message });
            }
        }
    }
}
