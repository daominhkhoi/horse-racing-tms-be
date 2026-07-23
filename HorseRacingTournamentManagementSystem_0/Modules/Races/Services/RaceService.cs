using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;
using HorseRacingTournamentManagementSystem_0.Database;
using System.Data;

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
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var race = await _context.Races
                .Include(r => r.RaceParticipants)
                .FirstOrDefaultAsync(r => r.RaceId == raceId);

            if (race == null) return false;

            if (race.Status != "Completed")
                throw new InvalidOperationException("Results can only be submitted for a completed race.");

            var resultsAlreadySubmitted = await _context.Results.AnyAsync(r => r.RaceId == raceId);
            if (resultsAlreadySubmitted)
                throw new InvalidOperationException("Race results have already been submitted and are permanently locked.");

            var approvedParticipantIds = race.RaceParticipants
                .Where(p => p.ParticipationStatus == "Approved")
                .Select(p => p.ParticipantId)
                .ToHashSet();
            var submittedResults = dto.ParticipantResults ?? new List<ParticipantResultDto>();
            var submittedParticipantIds = submittedResults.Select(r => r.ParticipantId).ToList();

            if (approvedParticipantIds.Count == 0)
                throw new ArgumentException("This race has no approved participants.");
            if (submittedParticipantIds.Count != approvedParticipantIds.Count
                || submittedParticipantIds.Distinct().Count() != submittedParticipantIds.Count
                || !submittedParticipantIds.ToHashSet().SetEquals(approvedParticipantIds))
                throw new ArgumentException("Results must be provided exactly once for every approved participant.");

            var ranks = submittedResults.Select(r => r.RankPosition).ToList();
            var expectedRanks = Enumerable.Range(1, approvedParticipantIds.Count).ToHashSet();
            if (ranks.Any(rank => !rank.HasValue)
                || ranks.Select(rank => rank!.Value).Distinct().Count() != approvedParticipantIds.Count
                || !ranks.Select(rank => rank!.Value).ToHashSet().SetEquals(expectedRanks))
                throw new ArgumentException($"Ranks must be unique and include every position from 1 to {approvedParticipantIds.Count}.");

            if (submittedResults.Any(r => !r.FinishTime.HasValue || r.FinishTime.Value == TimeOnly.MinValue))
                throw new ArgumentException("A valid finish time is required for every participant.");

            var allowedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Finished", "DNF", "DSQ" };
            if (submittedResults.Any(r => string.IsNullOrWhiteSpace(r.ResultStatus) || !allowedStatuses.Contains(r.ResultStatus)))
                throw new ArgumentException("Every participant must have a valid result status.");

            // Map and add new results
            foreach (var partResult in submittedResults)
            {
                // Verify the participant belongs to this race
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
            await transaction.CommitAsync();
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

        public async Task<string> AwardPrizesAsync(int raceId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var race = await _context.Races.FirstOrDefaultAsync(r => r.RaceId == raceId);
                if (race == null) return "Race not found.";
                if (race.Status == "Awarded") return "Prizes have already been awarded for this race.";
                if (race.Status != "Completed") return "Only completed races can be awarded.";

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
                        _context.PointTransactions.Add(new PointTransaction
                        {
                            SpectatorId = prediction.SpectatorId,
                            PredictionId = prediction.PredictionId,
                            Amount = prediction.RewardPoints ?? 0,
                            TransactionType = "BetWon",
                            Description = $"Winning reward for race #{raceId}."
                        });
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

            bool allFinished = tournament.Races.All(r => r.Status == "Completed" || r.Status == "Awarded" || r.Status == "Cancelled");
            bool anyActive = tournament.Races.Any(r => r.Status == "Started" || r.Status == "Live" || r.Status == "Ongoing" || r.Status == "Completed" || r.Status == "Awarded");

            string newStatus = "Upcoming";
            if (allFinished)
            {
                newStatus = "Completed";
            }
            else if (anyActive)
            {
                newStatus = "Live";
            }

            await _context.Tournaments
                .Where(t => t.TourId == tourId)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, newStatus));
        }

        public async Task<bool> UpdateYoutubeIdAsync(int raceId, string youtubeId)
        {
            var race = await _context.Races.FindAsync(raceId);
            if (race == null) return false;

            race.YoutubeId = youtubeId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RaceStreamDto>> GetActiveStreamsAsync()
        {
            var races = await _context.Races
                .Include(r => r.Tour)
                .Where(r => r.Status == "LIVE"
                    || r.Status == "UPCOMING"
                    || r.Status == "Racing"
                    || r.Status == "Open Registration"
                    || r.Status == "Registration Closed"
                    || r.Status == "Ready To Start")
                .ToListAsync();

            var dtos = races.Select(r => new RaceStreamDto
            {
                RaceId = r.RaceId,
                RaceName = r.RaceName,
                Track = r.Tour != null ? r.Tour.Location : "Unknown Track",
                Time = r.RaceDateTime.HasValue ? r.RaceDateTime.Value.ToString("HH:mm") : "TBD",
                Prize = r.Tour != null ? "$" + (r.Tour.PrizePool.HasValue ? r.Tour.PrizePool.Value.ToString("N0") : "0") : "$0",
                Status = r.Status,
                YoutubeId = r.YoutubeId,
                Leader = "N/A",
                Viewers = r.Status == "LIVE" || r.Status == "Racing" ? "Live" : "Waiting",
                TournamentName = r.Tour != null ? r.Tour.TourName : "Unknown Tournament"
            }).ToList();
            
            return dtos;
        }

        public async Task<List<object>> GetRaceCommentsAsync(int raceId)
        {
            var comments = await _context.RaceComments
                .Include(c => c.User)
                .Where(c => c.RaceId == raceId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => new
                {
                    user = c.User?.FullName ?? "Unknown",
                    text = c.Content,
                    time = c.CreatedAt.HasValue ? c.CreatedAt.Value.AddHours(7).ToString("HH:mm") : "Unknown",
                    hot = false
                }).Cast<object>().ToList();
        }

        public async Task<List<RefereeRaceDto>> GetRacesForRefereeAsync(int userId, bool isAdmin)
        {
            var query = _context.Races
                .AsNoTracking()
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(r => r.RefereeAssignments.Any(a => a.RefereeId == userId));
            }

            return await query
                .OrderByDescending(r => r.RaceDateTime)
                .Select(r => new RefereeRaceDto
                {
                    RaceId = r.RaceId,
                    RaceName = r.RaceName ?? "Unknown Race",
                    TournamentId = r.TourId,
                    TournamentName = r.Tour.TourName ?? "Unknown Tournament",
                    TournamentBanner = r.Tour.BannerUrl,
                    Track = r.Tour.Location ?? "Unknown Track",
                    Status = r.Status ?? "Pending",
                    HorsesCount = r.RaceParticipants.Count,
                    Laps = r.Round.HasValue ? r.Round.Value.ToString() : "0",
                    Leader = "—",
                    IncidentsCount = r.Violations.Count,
                    HasResults = r.Results.Any()
                })
                .ToListAsync();
        }

        public async Task<List<RefereeParticipantDto>> GetRaceParticipantsAsync(int raceId)
        {
            var participants = await _context.RaceParticipants
                .Include(p => p.Horse)
                .Include(p => p.Jockey)
                    .ThenInclude(j => j.User)
                .Where(p => p.RaceId == raceId)
                .Where(p => p.ParticipationStatus == "Approved")
                .ToListAsync();

            return participants.Select(p => new RefereeParticipantDto
            {
                ParticipantId = p.ParticipantId,
                HorseName = p.Horse?.HorseName ?? "Unknown Horse",
                JockeyName = p.Jockey?.User?.FullName ?? "Unknown Jockey" // Assuming Jockey has User relation if FullName is needed. Let's check this or use fallback.
            }).ToList();
        }

        public async Task<bool> ReportIncidentAsync(int raceId, int refereeId, CreateViolationDto dto)
        {
            var race = await _context.Races.FindAsync(raceId);
            if (race == null) return false;

            var violation = new HorseRacingTournamentManagementSystem_0.Entities.Violation
            {
                RaceId = raceId,
                ParticipantId = dto.ParticipantId,
                RefereeId = refereeId,
                ViolationType = dto.ViolationType,
                Penalty = dto.Penalty,
                Description = dto.Description
            };

            _context.Violations.Add(violation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<IncidentViewDto>> GetRaceIncidentsAsync(int raceId)
        {
            return await _context.Violations
                .AsNoTracking()
                .Where(v => v.RaceId == raceId)
                .OrderByDescending(v => v.ViolationId)
                .Select(v => new IncidentViewDto
                {
                    ViolationId = v.ViolationId,
                    ParticipantId = v.ParticipantId,
                    HorseName = v.Participant.Horse.HorseName ?? "Unknown Horse",
                    JockeyName = v.Participant.Jockey.User.FullName ?? "Unknown Jockey",
                    RefereeName = v.Referee.User.FullName ?? "Unknown Referee",
                    ViolationType = v.ViolationType ?? "Unspecified",
                    Penalty = v.Penalty ?? "None",
                    Description = v.Description
                })
                .ToListAsync();
        }
    }
}
