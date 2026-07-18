namespace HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs
{
    public class RefereeParticipantDto
    {
        public int ParticipantId { get; set; }
        public string HorseName { get; set; } = null!;
        public string JockeyName { get; set; } = null!;
    }
}
