using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Users.DTOs;

public class UpdateUserRequestDto
{
    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;

    public string? Phone { get; set; }
    public int? ExperienceYear { get; set; }
    public int? ExpYears { get; set; }
    public int? TotalPoints { get; set; }
    public bool RemoveAvatar { get; set; }
}
