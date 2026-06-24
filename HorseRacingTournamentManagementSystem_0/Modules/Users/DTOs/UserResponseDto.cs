namespace HorseRacingTournamentManagementSystem_0.Modules.Users.DTOs;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Status { get; set; } = null!;
}
