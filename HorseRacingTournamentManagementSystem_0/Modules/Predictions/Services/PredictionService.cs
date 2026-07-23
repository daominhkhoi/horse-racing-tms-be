using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Predictions.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Predictions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorseRacingTournamentManagementSystem_0.Modules.Predictions.Services;

public class PredictionService : IPredictionService
{
    private readonly HorseRacingDbContext _context;

    public PredictionService(HorseRacingDbContext context)
    {
        _context = context;
    }

    public async Task<string> PlaceBetAsync(int spectatorId, BetRequestDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var race = await _context.Races.FirstOrDefaultAsync(r => r.RaceId == request.RaceId);
            if (race == null) return "Race not found.";
            if (race.Status != "Open Registration")
                return "Betting is only available while race registration is open.";
            if (race.RaceDateTime.HasValue && race.RaceDateTime.Value <= DateTime.Now)
                return "Race time has passed.";

            var spectator = await _context.SpectatorProfiles.FirstOrDefaultAsync(s => s.UserId == spectatorId);
            if (spectator == null) return "Spectator profile not found.";

            if (spectator.TotalPoints == null || spectator.TotalPoints < request.BetPoints)
                return "Insufficient points.";

            var participantExists = await _context.RaceParticipants.AnyAsync(p => p.ParticipantId == request.ParticipantId && p.RaceId == request.RaceId);
            if (!participantExists) return "Participant not found in this race.";

            // Deduct points
            spectator.TotalPoints -= request.BetPoints;

            // Create or update prediction
            var existingPrediction = await _context.Predictions
                .FirstOrDefaultAsync(p => p.RaceId == request.RaceId 
                                       && p.SpectatorId == spectatorId 
                                       && p.ParticipantId == request.ParticipantId 
                                       && p.Status == "Active");

            Prediction prediction;
            if (existingPrediction != null)
            {
                existingPrediction.BetPoints += request.BetPoints;
                prediction = existingPrediction;
            }
            else
            {
                prediction = new Prediction
                {
                    RaceId = request.RaceId,
                    SpectatorId = spectatorId,
                    ParticipantId = request.ParticipantId,
                    BetPoints = request.BetPoints,
                    Status = "Active"
                };
                _context.Predictions.Add(prediction);
            }

            _context.PointTransactions.Add(new PointTransaction
            {
                SpectatorId = spectatorId,
                Prediction = prediction,
                Amount = -request.BetPoints,
                TransactionType = "BetPlaced",
                Description = $"Bet placed on race #{request.RaceId}."
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return "Bet placed successfully.";
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return "An error occurred while placing the bet.";
        }
    }

    public async Task<string> CancelBetAsync(int spectatorId, int predictionId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var prediction = await _context.Predictions
                .Include(p => p.Race)
                .FirstOrDefaultAsync(p => p.PredictionId == predictionId && p.SpectatorId == spectatorId);

            if (prediction == null) return "Prediction not found.";
            if (prediction.Status != "Active") return "Can only cancel active bets.";

            var race = prediction.Race;
            if (race.Status != "Open Registration")
                return "Cannot cancel bet after betting has closed.";
            if (race.RaceDateTime.HasValue && race.RaceDateTime.Value <= DateTime.Now)
                return "Cannot cancel bet. The race time has passed.";

            var spectator = await _context.SpectatorProfiles.FirstOrDefaultAsync(s => s.UserId == spectatorId);
            if (spectator == null) return "Spectator profile not found.";

            // Refund 50%
            double refund = prediction.BetPoints / 2.0;
            spectator.TotalPoints = (spectator.TotalPoints ?? 0) + refund;

            prediction.Status = "Cancelled";
            prediction.RewardPoints = 0;

            _context.PointTransactions.Add(new PointTransaction
            {
                SpectatorId = spectatorId,
                PredictionId = prediction.PredictionId,
                Amount = refund,
                TransactionType = "BetRefund",
                Description = "50% refund for a cancelled bet."
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return "Bet cancelled successfully. 50% points refunded.";
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return "An error occurred while cancelling the bet.";
        }
    }

    public async Task<IEnumerable<PredictionResponseDto>> GetMyPredictionsAsync(int spectatorId)
    {
        return await _context.Predictions
            .Include(p => p.Participant)
            .ThenInclude(pa => pa.Horse)
            .Where(p => p.SpectatorId == spectatorId)
            .OrderByDescending(p => p.PredictionId)
            .Select(p => new PredictionResponseDto
            {
                PredictionId = p.PredictionId,
                SpectatorId = p.SpectatorId,
                RaceId = p.RaceId,
                ParticipantId = p.ParticipantId,
                HorseName = p.Participant != null && p.Participant.Horse != null ? p.Participant.Horse.HorseName : null,
                HorseAvatar = p.Participant != null && p.Participant.Horse != null ? p.Participant.Horse.ImageUrl : null,
                BetPoints = p.BetPoints,
                Status = p.Status,
                RewardPoints = p.RewardPoints
            })
            .ToListAsync();
    }

    public async Task<List<AiPredictionDto>> GetAiInsightsAsync()
    {
        var participants = await _context.RaceParticipants
            .Include(rp => rp.Race)
            .Include(rp => rp.Horse)
            .Include(rp => rp.Jockey)
                .ThenInclude(jp => jp.User)
            .Where(rp => rp.Race.Status == "Upcoming" || rp.Race.Status == "Open Registration")
            .ToListAsync();

        var uniqueHorses = participants
            .GroupBy(rp => rp.HorseId)
            .Select(g => g.First())
            .ToList();

        var insights = new List<AiPredictionDto>();

        foreach (var rp in uniqueHorses)
        {
            var seededRandom = new Random(rp.HorseId ^ rp.RaceId);
            
            var pastResults = await _context.Results
                .Include(r => r.Race)
                .Include(r => r.Participant)
                .Where(r => r.Participant.HorseId == rp.HorseId && r.Race.Status == "Completed")
                .OrderByDescending(r => r.Race.RaceDateTime)
                .Take(5)
                .ToListAsync();

            var form = new List<string>();
            int confidence = seededRandom.Next(40, 96); // Default

            if (pastResults.Any())
            {
                foreach (var res in pastResults)
                {
                    form.Add(res.RankPosition == 1 ? "W" : "L");
                }
                
                int totalRaces = await _context.Results.CountAsync(r => r.Participant.HorseId == rp.HorseId && r.Race.Status == "Completed");
                if (totalRaces > 0)
                {
                    int totalWins = await _context.Results.CountAsync(r => r.Participant.HorseId == rp.HorseId && r.RankPosition == 1 && r.Race.Status == "Completed");
                    double realWinRate = (double)totalWins / totalRaces * 100;
                    confidence = (int)Math.Max(30, Math.Min(95, realWinRate + seededRandom.Next(-5, 15)));
                }
            }
            
            while (form.Count < 5)
            {
                form.Add(seededRandom.Next(3) == 0 ? "W" : "L");
            }
            form.Reverse(); // recent 5 races, reverse so oldest is first in the UI if needed, actually the UI usually shows oldest left, newest right. Wait, the query is OrderByDescending, so form[0] is most recent. UI displays left to right. Let's just reverse it.

            string status = confidence >= 80 ? "High Chance" : (confidence >= 60 ? "Moderate" : "Risky");
            double rawOdds = 100.0 / confidence * 1.5;
            string oddsStr = rawOdds.ToString("0.0") + "x";

            string gradient = "from-emerald-500 to-green-600";
            string avatarGrad = "from-sky-400 to-blue-600";
            
            if (status == "Moderate")
            {
                gradient = "from-amber-500 to-orange-600";
                avatarGrad = "from-amber-400 to-orange-600";
            }
            else if (status == "Risky")
            {
                gradient = "from-red-500 to-rose-600";
                avatarGrad = "from-red-400 to-rose-600";
            }

            insights.Add(new AiPredictionDto
            {
                Horse = rp.Horse.HorseName,
                Race = rp.Race.RaceName ?? "Race #" + rp.RaceId,
                Confidence = confidence,
                Odds = oddsStr,
                Status = status,
                Breed = rp.Horse.Breed ?? "Unknown",
                Jockey = rp.Jockey != null && rp.Jockey.User != null ? rp.Jockey.User.FullName : "Unknown",
                Track = "Main Track",
                Form = form,
                RaceDate = rp.Race.RaceDateTime.HasValue ? rp.Race.RaceDateTime.Value.ToString("dd MMM yyyy") : "TBA",
                Gradient = gradient,
                AvatarGrad = avatarGrad,
                ImageUrl = rp.Horse.ImageUrl
            });
        }

        return insights.OrderByDescending(i => i.Confidence).ToList();
    }
}
