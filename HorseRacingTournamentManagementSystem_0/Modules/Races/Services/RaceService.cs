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
            
            await SyncTournamentStatusAsync(race.TourId);

            return true;
        }

        public async Task<bool> SubmitRaceResultsAsync(int raceId, SubmitRaceResultDto dto)
        {
            var race = await _context.Races
                .Include(r => r.RaceParticipants)
                .FirstOrDefaultAsync(r => r.RaceId == raceId);

            if (race == null) return false;

            // Optional: you can add a check here to ensure the race status is 'Completed'
            if (race.Status == "Awarded") throw new Exception("Cannot edit results of an awarded race.");

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
                    JockeyName = r.Participant.Jockey != null ? r.Participant.Jockey.User.FullName : null,
                    JockeyAvatar = r.Participant.Jockey != null ? r.Participant.Jockey.Avatar : null,
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

        public async Task<string> AwardPrizesAsync(int raceId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var race = await _context.Races.FirstOrDefaultAsync(r => r.RaceId == raceId);
                if (race == null) return "Race not found.";
                if (race.Status == "Awarded") return "Prizes have already been awarded for this race.";

                var firstPlaceResults = await _context.Results
                    .Where(r => r.RaceId == raceId && r.RankPosition == 1)
                    .ToListAsync();
                if (!firstPlaceResults.Any()) return "Cannot award prizes. No 1st place result found.";
                
                var winningParticipantIds = firstPlaceResults.Select(r => r.ParticipantId).ToList();

                var predictions = await _context.Predictions
                    .Include(p => p.Spectator)
                    .Where(p => p.RaceId == raceId && p.Status == "Active")
                    .ToListAsync();

                double rewardRatio = race.RewardRatio ?? 2.0;

                foreach (var prediction in predictions)
                {
                    if (winningParticipantIds.Contains(prediction.ParticipantId))
                    {
                        prediction.Status = "Won";
                        prediction.RewardPoints = prediction.BetPoints * rewardRatio;
                        if (prediction.Spectator != null)
                        {
                            prediction.Spectator.TotalPoints = (prediction.Spectator.TotalPoints ?? 0) + prediction.RewardPoints;
                        }
                    }
                    else
                    {
                        prediction.Status = "Lost";
                        prediction.RewardPoints = 0;
                    }
                }

                race.Status = "Awarded";
                await _context.SaveChangesAsync();
                
                await SyncTournamentStatusAsync(race.TourId);

                await transaction.CommitAsync();

                return "Prizes awarded successfully.";
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                return "An error occurred while awarding prizes.";
            }
        }
        private async Task SyncTournamentStatusAsync(int tourId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Races)
                .FirstOrDefaultAsync(t => t.TourId == tourId);

            if (tournament == null) return;
            if (!tournament.Races.Any()) return;

            bool allFinished = tournament.Races.All(r => r.Status == "Completed" || r.Status == "Awarded");
            bool anyActive = tournament.Races.Any(r => r.Status == "Started" || r.Status == "Live" || r.Status == "Ongoing" || r.Status == "Completed" || r.Status == "Awarded");

            if (allFinished)
            {
                tournament.Status = "Completed";
            }
            else if (anyActive)
            {
                tournament.Status = "Live";
            }
            else
            {
                tournament.Status = "Upcoming";
            }

            await _context.SaveChangesAsync();
        }
    }
}
