using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HorseRacingTournamentManagementSystem_0.Modules.Leaderboards.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Leaderboards.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet("horses")]
    public async Task<IActionResult> GetHorsesLeaderboard()
    {
        var data = await _leaderboardService.GetGlobalHorseLeaderboardAsync();
        return Ok(new { data });
    }
}
