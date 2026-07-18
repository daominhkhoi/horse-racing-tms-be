using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;

public class RegisterHorseRequest
{
    [Required]
    public int HorseId { get; set; }
}

public class RegistrationReviewRequest
{
    public string? Reason { get; set; }
}

public class RaceRegistrationResponse
{
    public int RegistrationId { get; set; }
    public int RaceId { get; set; }
    public string? RaceName { get; set; }
    public int TourId { get; set; }
    public string? TourName { get; set; }
    public int HorseId { get; set; }
    public string? HorseName { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? Status { get; set; }
    public DateTime? RegisterTime { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectReason { get; set; }
    public int? InviteId { get; set; }
    public string? JockeyName { get; set; }
    public string? InvitationStatus { get; set; }
}

public class AvailableRaceHorseResponse
{
    public int HorseId { get; set; }
    public string? HorseName { get; set; }
    public string? Breed { get; set; }
    public string? ImageUrl { get; set; }
}

public class RaceRegistrationSummaryResponse
{
    public int RaceId { get; set; }
    public string? RaceStatus { get; set; }
    public int MinParticipants { get; set; }
    public int MaxParticipants { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int AcceptedJockeyCount { get; set; }
    public string? CancelReason { get; set; }
}
