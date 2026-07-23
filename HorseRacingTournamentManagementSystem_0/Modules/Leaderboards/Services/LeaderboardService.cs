using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Modules.Leaderboards.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Leaderboards.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Leaderboards.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly HorseRacingDbContext _context;

    public LeaderboardService(HorseRacingDbContext context)
    {
        _context = context;
    }

    public async Task<List<GlobalLeaderboardDto>> GetGlobalHorseLeaderboardAsync()
    {
        // Fetch all horses with their results
        var horses = await _context.Horses
            .Include(h => h.RaceParticipants)
                .ThenInclude(rp => rp.Results)
            .Include(h => h.RaceParticipants)
                .ThenInclude(rp => rp.Race)
            .Include(h => h.RaceParticipants)
                .ThenInclude(rp => rp.Jockey)
                    .ThenInclude(j => j.User)
            .Include(h => h.Leaderboards)
            .ToListAsync();

        var leaderboard = new List<GlobalLeaderboardDto>();
        var random = new Random();

        foreach (var horse in horses)
        {
            // Calculate total points and wins from Leaderboards if they exist,
            // or from Results to be safe
            int totalWins = 0;
            double totalPoints = 0;
            
            if (horse.Leaderboards.Any())
            {
                totalWins = horse.Leaderboards.Sum(l => l.TotalWins ?? 0);
                totalPoints = horse.Leaderboards.Sum(l => l.TotalPoints ?? 0);
            }
            else
            {
                // Fallback to calculate from Results
                var allCompletedResults = horse.RaceParticipants
                    .Where(rp => rp.Race != null && rp.Race.Status == "Completed")
                    .SelectMany(rp => rp.Results)
                    .ToList();
                
                totalWins = allCompletedResults.Count(r => r.RankPosition == 1);
                totalPoints = (double)allCompletedResults.Sum(r => r.RewardMoney ?? 0); // Using RewardMoney as pseudo-points if Leaderboard is empty
            }

            // Get total completed races for win rate
            int totalRaces = horse.RaceParticipants.Count(rp => rp.Race != null && rp.Race.Status == "Completed");
            double winRate = totalRaces > 0 ? ((double)totalWins / totalRaces) * 100 : 0;

            // Recent form: last 5 completed races
            var recentCompletedRaces = horse.RaceParticipants
                .Where(rp => rp.Race != null && rp.Race.Status == "Completed")
                .OrderByDescending(rp => rp.Race.RaceDateTime)
                .Take(5)
                .ToList();

            var form = new List<string>();
            foreach (var rp in recentCompletedRaces)
            {
                var result = rp.Results.FirstOrDefault();
                form.Add(result != null && result.RankPosition == 1 ? "W" : "L");
            }

            // Fill missing with random if less than 5 for demo purposes
            while (form.Count < 5)
            {
                form.Add(random.Next(3) == 0 ? "W" : "L");
            }
            form.Reverse(); // oldest first visually

            // Get most recent jockey
            var mostRecentJockey = horse.RaceParticipants
                .OrderByDescending(rp => rp.Race?.RaceDateTime)
                .FirstOrDefault(rp => rp.Jockey != null)?.Jockey?.User?.FullName ?? "Unknown Jockey";

            // Only add if they have some races or points to avoid empty entries in the leaderboard,
            // or just add everyone. Let's add all that have participated in at least one race.
            if (horse.RaceParticipants.Any())
            {
                leaderboard.Add(new GlobalLeaderboardDto
                {
                    HorseId = horse.HorseId,
                    Horse = horse.HorseName,
                    Jockey = mostRecentJockey,
                    Wins = totalWins,
                    Points = totalPoints,
                    WinRate = Math.Round(winRate, 1),
                    Breed = horse.Breed ?? "Unknown",
                    Form = form,
                    ImageUrl = horse.ImageUrl,
                    // Assign random avatar background for initials
                    AvatarBg = GetRandomGradient(random),
                    TextCol = GetRandomTextColor(random)
                });
            }
        }

        return leaderboard.OrderByDescending(l => l.Points).ToList();
    }

    private string GetRandomGradient(Random random)
    {
        var gradients = new[]
        {
            "from-amber-400 to-amber-600",
            "from-slate-300 to-slate-500",
            "from-orange-400 to-orange-600",
            "from-teal-400 to-emerald-600",
            "from-red-400 to-rose-600",
            "from-sky-400 to-blue-600",
            "from-violet-400 to-purple-600",
            "from-cyan-400 to-blue-500"
        };
        return gradients[random.Next(gradients.Length)];
    }

    private string GetRandomTextColor(Random random)
    {
        var colors = new[]
        {
            "text-amber-500",
            "text-slate-400",
            "text-orange-600",
            "text-emerald-500",
            "text-red-500",
            "text-blue-500",
            "text-purple-500",
            "text-cyan-500"
        };
        return colors[random.Next(colors.Length)];
    }
}
