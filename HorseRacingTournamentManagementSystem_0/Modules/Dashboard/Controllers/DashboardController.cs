using HorseRacingTournamentManagementSystem_0.Modules.Dashboard.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Dashboard.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HorseRacingTournamentManagementSystem_0.Modules.Dashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var stats = await _dashboardService.GetStatsAsync();
        return Ok(stats);
    }

    [HttpGet("chart")]
    public async Task<ActionResult<List<ChartDataDto>>> GetChartData([FromQuery] string type = "races", [FromQuery] string range = "W")
    {
        var chartData = await _dashboardService.GetChartDataAsync(type, range);
        return Ok(chartData);
    }
}
