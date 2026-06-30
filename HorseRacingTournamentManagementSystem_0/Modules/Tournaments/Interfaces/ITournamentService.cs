using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Tournaments.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Common.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Interfaces
{
    public interface ITournamentService
    {
        Task<PagedResult<TournamentDto>> GetAllTournamentsAsync(int page, int pageSize, string? searchTerm);
        Task<TournamentDetailDto> GetTournamentByIdAsync(int id);
        Task<Tournament> CreateTournamentAsync(CreateTournamentDto dto);
    }
}
