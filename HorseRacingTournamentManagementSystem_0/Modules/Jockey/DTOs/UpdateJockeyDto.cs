using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs
{
    public class UpdateJockeyDto
    {
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [MaxLength(255, ErrorMessage = "Image URL cannot exceed 255 characters")]
        public string? Avatar { get; set; }

        [Range(0, 60, ErrorMessage = "Experience years must be between 0 and 60")]
        public int? ExperienceYear { get; set; }
    }
}
