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
    }
}
