using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Leaderboards.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Leaderboards.Interfaces;

public interface ILeaderboardService
{
    Task<List<GlobalLeaderboardDto>> GetGlobalHorseLeaderboardAsync();
}
