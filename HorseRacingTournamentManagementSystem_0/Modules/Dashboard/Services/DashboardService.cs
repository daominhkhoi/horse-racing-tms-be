using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Modules.Dashboard.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Dashboard.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorseRacingTournamentManagementSystem_0.Modules.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly HorseRacingDbContext _context;

    public DashboardService(HorseRacingDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var totalHorses = await _context.Horses.CountAsync();
        var activeTournaments = await _context.Tournaments.CountAsync(t => t.Status == "Live" || t.Status == "Ongoing" || t.Status == "Active");
        var totalJockeys = await _context.JockeyProfiles.CountAsync();
        var completedTournaments = await _context.Tournaments.CountAsync(t => t.Status == "Completed");

        return new DashboardStatsDto
        {
            TotalHorses = totalHorses,
            ActiveTournaments = activeTournaments,
            TotalJockeys = totalJockeys,
            CompletedTournaments = completedTournaments
        };
    }
}
