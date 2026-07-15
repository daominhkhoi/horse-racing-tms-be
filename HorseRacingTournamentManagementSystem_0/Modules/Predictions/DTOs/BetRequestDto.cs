namespace HorseRacingTournamentManagementSystem_0.Modules.Predictions.DTOs;

public class BetRequestDto
{
    public int RaceId { get; set; }
    public int ParticipantId { get; set; }
    public double BetPoints { get; set; }
}
