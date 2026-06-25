using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Common.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Users.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Users.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Users.Services;

public class UserService : IUserService
{
    private readonly HorseRacingDbContext _context;

    public UserService(HorseRacingDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserResponseDto>> GetUsersAsync(string searchKeyword, string roleName, int page, int pageSize)
    {
        var query = _context.Users.Include(u => u.Role).AsQueryable();

        // Filter by role
        if (!string.IsNullOrWhiteSpace(roleName) && roleName != "All")
        {
            query = query.Where(u => u.Role.RoleName == roleName);
        }

        // Filter by search keyword (Name or Email)
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var lowerSearch = searchKeyword.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(lowerSearch) || u.Email.ToLower().Contains(lowerSearch));
        }

        // Total count for pagination
        var totalCount = await query.CountAsync();

        // Paginate
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponseDto
            {
                Id = u.UserId,
                Name = u.FullName,
                Email = u.Email,
                Role = u.Role != null ? u.Role.RoleName : "Unknown",
                Status = u.IsActive == true ? "Active" : "Inactive"
            })
            .ToListAsync();

        return new PagedResult<UserResponseDto>
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ToggleUserStatusAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        user.IsActive = !(user.IsActive ?? true);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserRequestDto request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;

        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email && u.UserId != id);
        if (emailExists)
            throw new System.Exception("Email is already in use by another user.");

        user.FullName = request.FullName;
        user.Email = request.Email;

        var role = await _context.Roles.SingleOrDefaultAsync(r => r.RoleName == request.Role);
        if (role != null)
        {
            user.RoleId = role.RoleId;
        }
        else
        {
            throw new System.Exception("The specified role does not exist.");
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
