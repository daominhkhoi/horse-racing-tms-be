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
        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.AdminProfile)
            .Include(u => u.JockeyProfile)
            .Include(u => u.OwnerProfile)
            .Include(u => u.RefereeProfile)
            .Include(u => u.SpectatorProfile)
            .AsQueryable();

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
                Status = u.IsActive == true ? "Active" : "Inactive",
                Phone = u.JockeyProfile != null ? u.JockeyProfile.Phone :
                        u.OwnerProfile != null ? u.OwnerProfile.Phone :
                        u.RefereeProfile != null ? u.RefereeProfile.Phone :
                        u.SpectatorProfile != null ? u.SpectatorProfile.Phone :
                        u.AdminProfile != null ? u.AdminProfile.Phone : null,
                Avatar = u.JockeyProfile != null ? u.JockeyProfile.Avatar :
                         u.OwnerProfile != null ? u.OwnerProfile.Avatar :
                         u.RefereeProfile != null ? u.RefereeProfile.Avatar :
                         u.SpectatorProfile != null ? u.SpectatorProfile.Avatar :
                         u.AdminProfile != null ? u.AdminProfile.Avatar : null,
                Weight = u.JockeyProfile != null ? u.JockeyProfile.Weight : null,
                ExperienceYear = u.JockeyProfile != null ? u.JockeyProfile.ExperienceYear : null,
                ExpYears = u.RefereeProfile != null ? u.RefereeProfile.ExpYears : 
                           (u.JockeyProfile != null ? u.JockeyProfile.ExpYears : null),
                TotalPoints = u.SpectatorProfile != null ? u.SpectatorProfile.TotalPoints : null
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
        var user = await _context.Users
            .Include(u => u.AdminProfile)
            .Include(u => u.JockeyProfile)
            .Include(u => u.OwnerProfile)
            .Include(u => u.RefereeProfile)
            .Include(u => u.SpectatorProfile)
            .FirstOrDefaultAsync(u => u.UserId == id);
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

            if (request.Role == "Jockey")
            {
                if (user.JockeyProfile == null) { user.JockeyProfile = new JockeyProfile { UserId = id }; }
                user.JockeyProfile.Phone = request.Phone;
                if (request.RemoveAvatar) user.JockeyProfile.Avatar = null;
                user.JockeyProfile.Weight = request.Weight;
                user.JockeyProfile.ExperienceYear = request.ExperienceYear;
                user.JockeyProfile.ExpYears = request.ExpYears;
            }
            else if (request.Role == "HorseOwner")
            {
                if (user.OwnerProfile == null) { user.OwnerProfile = new OwnerProfile { UserId = id }; }
                user.OwnerProfile.Phone = request.Phone;
                if (request.RemoveAvatar) user.OwnerProfile.Avatar = null;
            }
            else if (request.Role == "Referee")
            {
                if (user.RefereeProfile == null) { user.RefereeProfile = new RefereeProfile { UserId = id }; }
                user.RefereeProfile.Phone = request.Phone;
                if (request.RemoveAvatar) user.RefereeProfile.Avatar = null;
                user.RefereeProfile.ExpYears = request.ExpYears;
            }
            else if (request.Role == "Spectator")
            {
                if (user.SpectatorProfile == null) { user.SpectatorProfile = new SpectatorProfile { UserId = id }; }
                user.SpectatorProfile.Phone = request.Phone;
                if (request.RemoveAvatar) user.SpectatorProfile.Avatar = null;
                if (request.TotalPoints.HasValue) user.SpectatorProfile.TotalPoints = request.TotalPoints.Value;
            }
            else if (request.Role == "Admin")
            {
                if (user.AdminProfile == null) { user.AdminProfile = new AdminProfile { UserId = id }; }
                user.AdminProfile.Phone = request.Phone;
                if (request.RemoveAvatar) user.AdminProfile.Avatar = null;
            }
        }
        else
        {
            throw new System.Exception("The specified role does not exist.");
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
