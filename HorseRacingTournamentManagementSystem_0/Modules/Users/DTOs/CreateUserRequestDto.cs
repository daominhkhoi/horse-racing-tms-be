using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Users.DTOs;

public class CreateUserRequestDto
{
    [Required(ErrorMessage = "Full Name is required.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;

    public string? Phone { get; set; }

    // Role-specific fields
    public int? ExperienceYear { get; set; }
    public int? ExpYears { get; set; }
    public double? TotalPoints { get; set; }
}
