using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;
using HorseRacingTournamentManagementSystem_0.Database;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Services
{
    public class RaceService : IRaceService
    {
        private readonly HorseRacingDbContext _context;

        public RaceService(HorseRacingDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateRaceStatusAsync(int raceId, string newStatus)
        {
            var race = await _context.Races.FindAsync(raceId);
            if (race == null) return false;

            race.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitRaceResultsAsync(int raceId, SubmitRaceResultDto dto)
        {
            var race = await _context.Races
                .Include(r => r.RaceParticipants)
                .FirstOrDefaultAsync(r => r.RaceId == raceId);

            if (race == null) return false;

            // Optional: you can add a check here to ensure the race status is 'Completed'
            // if (race.Status != "Completed") return false;

            // Clear existing results for this race
            var existingResults = await _context.Results.Where(r => r.RaceId == raceId).ToListAsync();
            if (existingResults.Any())
            {
                _context.Results.RemoveRange(existingResults);
            }

            // Map and add new results
            foreach (var partResult in dto.ParticipantResults)
            {
                // Verify the participant belongs to this race
                if (!race.RaceParticipants.Any(p => p.ParticipantId == partResult.ParticipantId))
                    continue;

                var result = new Result
                {
                    RaceId = raceId,
                    ParticipantId = partResult.ParticipantId,
                    RankPosition = partResult.RankPosition,
                    FinishTime = partResult.FinishTime,
                    ResultStatus = partResult.ResultStatus
                };

                _context.Results.Add(result);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ResultViewDto>> GetRaceResultsAsync(int raceId)
        {
            var results = await _context.Results
                .Include(r => r.Participant)
                    .ThenInclude(p => p.Horse)
                .Include(r => r.Participant)
                    .ThenInclude(p => p.Jockey)
                        .ThenInclude(j => j.User) // Assuming User contains Avatar/Name
                .Where(r => r.RaceId == raceId)
                .Select(r => new ResultViewDto
                {
                    ResultId = r.ResultId,
                    ParticipantId = r.ParticipantId,
                    HorseId = r.Participant.HorseId,
                    HorseName = r.Participant.Horse.HorseName,
                    HorseAvatar = r.Participant.Horse.ImageUrl,
                    JockeyId = r.Participant.JockeyId,
                    JockeyName = r.Participant.Jockey.User.FullName, // Assuming FullName is in User
                    JockeyAvatar = r.Participant.Jockey.Avatar,
                    LaneNumber = r.Participant.LaneNumber,
                    FinishTime = r.FinishTime,
                    RankPosition = r.RankPosition,
                    ResultStatus = r.ResultStatus,
                    RewardMoney = r.RewardMoney
                })
                .OrderBy(r => r.RankPosition.HasValue ? 0 : 1)
                .ThenBy(r => r.RankPosition)
                .ToListAsync();

            return results;
        }
    }
}
