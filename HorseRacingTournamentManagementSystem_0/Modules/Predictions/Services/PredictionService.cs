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

            if (existingPrediction != null)
            {
                existingPrediction.BetPoints += request.BetPoints;
            }
            else
            {
                var prediction = new Prediction
                {
                    RaceId = request.RaceId,
                    SpectatorId = spectatorId,
                    ParticipantId = request.ParticipantId,
                    BetPoints = request.BetPoints,
                    Status = "Active"
                };
                _context.Predictions.Add(prediction);
            }

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
}
