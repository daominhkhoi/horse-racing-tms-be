using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs
{
    public class CreateViolationDto
    {
        [Required]
        public int ParticipantId { get; set; }
        
        [Required]
        public string ViolationType { get; set; } = null!;
        
        [Required]
        public string Penalty { get; set; } = null!;
        
        public string? Description { get; set; }
    }
}
