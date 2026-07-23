using HorseRacingTournamentManagementSystem_0.Modules.Predictions.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Predictions.Interfaces;

public interface IPredictionService
{
    Task<string> PlaceBetAsync(int spectatorId, BetRequestDto request);
    Task<string> CancelBetAsync(int spectatorId, int predictionId);
    Task<IEnumerable<PredictionResponseDto>> GetMyPredictionsAsync(int spectatorId);
    Task<List<AiPredictionDto>> GetAiInsightsAsync();
}
