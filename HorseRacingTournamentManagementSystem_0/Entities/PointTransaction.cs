namespace HorseRacingTournamentManagementSystem_0.Entities;

public class PointTransaction
{
    public int TransactionId { get; set; }
    public int SpectatorId { get; set; }
    public int? PredictionId { get; set; }
    public double Amount { get; set; }
    public string TransactionType { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual SpectatorProfile Spectator { get; set; } = null!;
    public virtual Prediction? Prediction { get; set; }
}
