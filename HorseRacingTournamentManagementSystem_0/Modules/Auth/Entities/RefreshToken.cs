namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}