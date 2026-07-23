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

    public async Task<List<ChartDataDto>> GetChartDataAsync(string type, string range)
    {
        var result = new List<ChartDataDto>();
        var today = DateTime.Today;
        
        DateTime startDate;
        DateTime endDate;
        List<(string Label, DateTime Start, DateTime End)> segments = new();
        string rangeUpper = (range ?? "W").ToUpper();

        if (rangeUpper == "Y")
        {
            // Last 12 months
            startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
            endDate = new DateTime(today.Year, today.Month, 1).AddMonths(1);
            for (int i = 0; i < 12; i++)
            {
                var s = startDate.AddMonths(i);
                var e = s.AddMonths(1);
                segments.Add((s.ToString("MMM"), s, e));
            }
        }
        else if (rangeUpper == "M")
        {
            // Last 4 weeks
            startDate = today.AddDays(-27);
            endDate = today.AddDays(1);
            for (int i = 0; i < 4; i++)
            {
                var s = startDate.AddDays(i * 7);
                var e = s.AddDays(7);
                segments.Add(($"Week {i + 1}", s, e));
            }
        }
        else if (rangeUpper == "D")
        {
            // Today vs Yesterday vs ... (Last 4 days for simplicity)
            // Or just last 5 days
            startDate = today.AddDays(-4);
            endDate = today.AddDays(1);
            for (int i = 0; i < 5; i++)
            {
                var s = startDate.AddDays(i);
                var e = s.AddDays(1);
                segments.Add((s.ToString("dd/MM"), s, e));
            }
        }
        else // "W"
        {
            // Last 7 days
            startDate = today.AddDays(-6);
            endDate = today.AddDays(1);
            for (int i = 0; i < 7; i++)
            {
                var s = startDate.AddDays(i);
                var e = s.AddDays(1);
                segments.Add((s.ToString("ddd"), s, e));
            }
        }

        var typeLower = (type ?? "races").ToLower();
        List<DateTime?> dates = new();

        if (typeLower == "predictions")
        {
            dates = await _context.Predictions
                .Include(p => p.Race)
                .Where(p => p.Race.RaceDateTime >= startDate && p.Race.RaceDateTime < endDate)
                .Select(p => p.Race.RaceDateTime)
                .ToListAsync();
        }
        else if (typeLower == "participants")
        {
            dates = await _context.RaceParticipants
                .Include(rp => rp.Race)
                .Where(rp => rp.Race.RaceDateTime >= startDate && rp.Race.RaceDateTime < endDate)
                .Select(rp => rp.Race.RaceDateTime)
                .ToListAsync();
        }
        else
        {
            dates = await _context.Races
                .Where(r => r.RaceDateTime >= startDate && r.RaceDateTime < endDate)
                .Select(r => r.RaceDateTime)
                .ToListAsync();
        }

        var validDates = dates.Where(d => d.HasValue).Select(d => d.Value).ToList();

        foreach (var segment in segments)
        {
            int count = validDates.Count(d => d >= segment.Start && d < segment.End);
            result.Add(new ChartDataDto
            {
                Label = segment.Label,
                Value = count
            });
        }

        return result;
    }
}
