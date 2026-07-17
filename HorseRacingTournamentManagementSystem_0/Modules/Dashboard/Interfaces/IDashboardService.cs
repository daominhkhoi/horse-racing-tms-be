using HorseRacingTournamentManagementSystem_0.Modules.Dashboard.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
