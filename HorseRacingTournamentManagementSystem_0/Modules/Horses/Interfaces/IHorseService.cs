using System.Threading.Tasks;
using System.Collections.Generic;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Horses.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Horses.Interfaces
{
    public interface IHorseService
    {
        Task<Horse> RegisterHorseAsync(CreateHorseDto createHorseDto);
        Task<IEnumerable<Horse>> GetHorsesByOwnerAsync(int ownerId);
        Task<IEnumerable<Horse>> GetAllHorsesAsync();
        Task<Horse> VerifyHorseAsync(int horseId, VerifyHorseDto dto);
        Task<bool> DeleteHorseAsync(int horseId);
        Task<bool> UpdateHorseStatusAsync(int horseId, string status);
        Task<bool> RequestUpdateHorseAsync(int horseId, UpdateHorseDto dto);
        Task<bool> ApproveUpdateHorseAsync(int horseId, int adminId, string notes);
        Task<bool> RejectUpdateHorseAsync(int horseId, int adminId, string notes);
    }
}
