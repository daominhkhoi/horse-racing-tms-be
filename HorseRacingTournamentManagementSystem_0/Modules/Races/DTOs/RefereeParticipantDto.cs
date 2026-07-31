namespace HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs
{
    public class RefereeParticipantDto
    {
        public int ParticipantId { get; set; }
        public string HorseName { get; set; } = null!;
        public int HorseId { get; set; }
        public string? HorseAvatar { get; set; }
        public string JockeyName { get; set; } = null!;
        public int? LaneNumber { get; set; }
    }
}
