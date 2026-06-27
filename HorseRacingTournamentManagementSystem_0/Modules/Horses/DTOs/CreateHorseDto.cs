using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Horses.DTOs
{
    public class CreateHorseDto
    {
        [Required]
        public int OwnerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string HorseName { get; set; } = null!;

        [MaxLength(100)]
        public string? Breed { get; set; }

        public int? Age { get; set; }

        public double? Weight { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(50)]
        public string? HealthStatus { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        [MaxLength(255)]
        public string? InspectionUrl { get; set; }

        [MaxLength(255)]
        public string? HealthCertUrl { get; set; }
    }
}
