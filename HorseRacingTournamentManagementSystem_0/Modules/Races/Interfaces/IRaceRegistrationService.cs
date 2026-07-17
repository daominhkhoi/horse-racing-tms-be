using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces
{
    public interface IRaceRegistrationService
    {
        Task<RaceRegistrationDto> RegisterHorseAsync(int ownerId, int raceId, int horseId);
        Task<bool> ApproveRegistrationAsync(int registrationId);
        Task<bool> RejectRegistrationAsync(int registrationId, string? note);
        Task<List<RaceRegistrationDto>> GetRegistrationsByRaceAsync(int raceId);
        Task<List<RaceRegistrationDto>> GetMyRegistrationsAsync(int ownerId);
        Task<bool> OpenRegistrationAsync(int raceId);
        Task<bool> CloseRegistrationAsync(int raceId);
        Task<RaceStatusDto> GetRaceStatusAsync(int raceId);
        Task<List<ApprovedParticipantDto>> GetApprovedParticipantsAsync(int raceId);
        Task<bool> ApproveJockeyAsync(int participantId);
    }
}

