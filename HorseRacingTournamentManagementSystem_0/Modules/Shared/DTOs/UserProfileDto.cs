namespace HorseRacingTournamentManagementSystem_0.Modules.Shared.DTOs;

public class UserProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? JoinedDate { get; set; }
    public bool IsActive { get; set; }
    public double? TotalPoints { get; set; }
}
