namespace HorseRacingTournamentManagementSystem_0.Modules.Users.DTOs;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Status { get; set; } = null!;

    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public int? ExperienceYear { get; set; }
    public int? ExpYears { get; set; }
    public int? TotalPoints { get; set; }
}
