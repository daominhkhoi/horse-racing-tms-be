namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class RaceRegistration
{
    public int RegistrationId { get; set; }
    public int RaceId { get; set; }
    public int HorseId { get; set; }
    public int OwnerId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? RegisterTime { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public string? RejectReason { get; set; }

    public virtual Race Race { get; set; } = null!;
    public virtual Horse Horse { get; set; } = null!;
    public virtual OwnerProfile Owner { get; set; } = null!;
    public virtual User? ReviewedByNavigation { get; set; }
}
