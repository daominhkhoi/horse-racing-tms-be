using HorseRacingTournamentManagementSystem_0.Modules.Auth.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"];

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("SendGrid API Key is not configured.");

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName ?? "Horse Racing TMS");
            var subject = "Password Reset Request";
            var to = new EmailAddress(toEmail);
            
            var plainTextContent = $"Please reset your password by clicking here: {resetLink}";
            var htmlContent = $@"
                <h3>Password Reset</h3>
                <p>You have requested to reset your password.</p>
                <p>Please click the link below to set a new password:</p>
                <p><a href='{resetLink}'>{resetLink}</a></p>
                <p>If you did not request a password reset, please ignore this email.</p>
            ";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            await client.SendEmailAsync(msg);
        }
    }
}
