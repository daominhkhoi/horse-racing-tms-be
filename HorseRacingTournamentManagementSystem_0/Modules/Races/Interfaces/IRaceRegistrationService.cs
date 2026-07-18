using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;

public interface IRaceRegistrationService
{
    Task<RaceRegistrationResponse> RegisterHorseAsync(int ownerId, int raceId, RegisterHorseRequest request);
    Task<IEnumerable<AvailableRaceHorseResponse>> GetAvailableHorsesAsync(int ownerId, int raceId);
    Task<RaceRegistrationResponse> ReviewRegistrationAsync(int adminId, int registrationId, bool approved, string? reason);
    Task<IEnumerable<RaceRegistrationResponse>> GetRegistrationsAsync(int raceId, string? status);
    Task<IEnumerable<RaceRegistrationResponse>> GetMyRegistrationsAsync(int ownerId);
    Task<RaceRegistrationSummaryResponse> GetSummaryAsync(int raceId);
    Task SetRegistrationStatusAsync(int raceId, string status);
    Task StartRaceAsync(int raceId);
    Task<bool> HasApprovedRegistrationAsync(int ownerId, int horseId, int tourId);
    Task SyncAcceptedInvitationAsync(int horseId, int jockeyId, int tourId);
}
