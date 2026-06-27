using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Horses.DTOs
{
    public class UpdateHorseDto
    {
        [Required(ErrorMessage = "Tên ngựa không được để trống")]
        public string HorseName { get; set; } = null!;

        public string? Breed { get; set; }

        public int? Age { get; set; }

        public int? Weight { get; set; }

        public string? Gender { get; set; }

        public string? HealthStatus { get; set; }

        public string? ImageUrl { get; set; }

        public string? Status { get; set; }
    }
}
