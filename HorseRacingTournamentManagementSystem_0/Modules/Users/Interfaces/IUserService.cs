using HorseRacingTournamentManagementSystem_0.Modules.Common.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Users.DTOs;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Users.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserResponseDto>> GetUsersAsync(string searchKeyword, string roleName, int page, int pageSize);
    Task<bool> ToggleUserStatusAsync(int id);
}
