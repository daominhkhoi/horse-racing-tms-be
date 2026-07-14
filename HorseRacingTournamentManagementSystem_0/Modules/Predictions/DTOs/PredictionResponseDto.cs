namespace HorseRacingTournamentManagementSystem_0.Modules.Predictions.DTOs;

public class PredictionResponseDto
{
    public int PredictionId { get; set; }
    public int SpectatorId { get; set; }
    public int RaceId { get; set; }
    public int ParticipantId { get; set; }
    public string? HorseName { get; set; }
    public string? HorseAvatar { get; set; }
    public double BetPoints { get; set; }
    public string? Status { get; set; }
    public double? RewardPoints { get; set; }
    public DateTime? CreateAt { get; set; }
}
