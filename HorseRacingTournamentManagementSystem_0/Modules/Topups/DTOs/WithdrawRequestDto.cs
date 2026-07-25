namespace HorseRacingTournamentManagementSystem_0.Modules.Topups.DTOs;

public class WithdrawRequestDto
{
    public double Amount { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
}
