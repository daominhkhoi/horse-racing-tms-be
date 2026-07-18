namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class User
{
    public string FullName { get; set; } = null!;

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AdminProfile? AdminProfile { get; set; }

    public virtual JockeyProfile? JockeyProfile { get; set; }

    public virtual OwnerProfile? OwnerProfile { get; set; }

    public virtual RefereeProfile? RefereeProfile { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<HorseVerification> HorseVerifications { get; set; } = new List<HorseVerification>();

    public virtual Role Role { get; set; } = null!;

    public virtual SpectatorProfile? SpectatorProfile { get; set; }

    public virtual ICollection<RaceComment> RaceComments { get; set; } = new List<RaceComment>();
}
