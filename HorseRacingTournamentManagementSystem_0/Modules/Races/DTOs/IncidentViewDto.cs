namespace HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs
{
    public class IncidentViewDto
    {
        public int ViolationId { get; set; }
        public int ParticipantId { get; set; }
        public string HorseName { get; set; } = string.Empty;
        public string JockeyName { get; set; } = string.Empty;
        public string RefereeName { get; set; } = string.Empty;
        public string ViolationType { get; set; } = string.Empty;
        public string Penalty { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
