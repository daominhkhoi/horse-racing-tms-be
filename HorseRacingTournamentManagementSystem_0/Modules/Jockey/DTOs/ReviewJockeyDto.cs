using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs
{
    public class ReviewJockeyDto
    {
        [Required(ErrorMessage = "IsApproved is required (true = approve / false = reject)")]
        public bool IsApproved { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "ReviewedBy is required")]
        public int ReviewedBy { get; set; }
    }
}
