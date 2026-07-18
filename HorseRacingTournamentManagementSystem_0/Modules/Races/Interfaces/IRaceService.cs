using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces
{
    public interface IRaceService
    {
        Task<bool> UpdateRaceStatusAsync(int raceId, string newStatus);
        Task<bool> SubmitRaceResultsAsync(int raceId, SubmitRaceResultDto dto);
        Task<List<ResultViewDto>> GetRaceResultsAsync(int raceId);
        Task<string> AwardPrizesAsync(int raceId);
        Task<bool> UpdateYoutubeIdAsync(int raceId, string youtubeId);
        Task<List<RaceStreamDto>> GetActiveStreamsAsync();
        Task<List<object>> GetRaceCommentsAsync(int raceId);
        Task<List<RefereeRaceDto>> GetRacesForRefereeAsync(int userId, bool isAdmin);
        Task<List<RefereeParticipantDto>> GetRaceParticipantsAsync(int raceId);
        Task<bool> ReportIncidentAsync(int raceId, int refereeId, CreateViolationDto dto);
    }
}
