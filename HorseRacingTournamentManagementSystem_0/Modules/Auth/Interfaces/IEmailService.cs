namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }
}
