using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Tournaments.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Interfaces
{
    public interface ITournamentService
    {
        Task<Tournament> CreateTournamentAsync(CreateTournamentDto dto);
    }
}
