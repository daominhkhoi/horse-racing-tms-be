namespace HorseRacingTournamentManagementSystem_0.Modules.Predictions.DTOs;

public class AnonymousPredictionDto
{
    public int PredictionId { get; set; }
    public string BettorAlias { get; set; } = null!;
    public int TournamentId { get; set; }
    public string TournamentName { get; set; } = null!;
    public string RaceName { get; set; } = null!;
    public string HorseName { get; set; } = null!;
    public string? HorseAvatar { get; set; }
    public double Odds { get; set; }
    public double BetPoints { get; set; }
    public string Status { get; set; } = null!;
    public double? ProfitLoss { get; set; }
    public DateTime? BetPlacedAt { get; set; }
    public DateTime? RaceDateTime { get; set; }
}
