using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.Interfaces
{
    public interface IJockeyService
    {
        Task<bool> RequestUpdateProfileAsync(int jockeyId, UpdateJockeyDto dto);

        Task<bool> ReviewUpdateRequestAsync(int jockeyId, ReviewJockeyDto dto);

        Task<IEnumerable<JockeyProfile>> GetAllJockeysPublicAsync();

        Task<IEnumerable<JockeyProfile>> GetAvailableJockeysForRaceAsync(int raceId);
    }
}
