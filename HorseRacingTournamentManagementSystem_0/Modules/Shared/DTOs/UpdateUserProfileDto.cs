namespace HorseRacingTournamentManagementSystem_0.Modules.Shared.DTOs;

public class UpdateUserProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
}
