using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Horses.DTOs
{
    public class VerifyHorseDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = null!;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int VerifiedBy { get; set; }
    }
}
