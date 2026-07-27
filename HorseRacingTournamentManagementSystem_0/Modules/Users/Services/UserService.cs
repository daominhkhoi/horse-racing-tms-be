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
                ExperienceYear = u.JockeyProfile != null ? u.JockeyProfile.ExperienceYear : null,
                ExpYears = u.RefereeProfile != null ? u.RefereeProfile.ExpYears : null,
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

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
            return false;

        if (user.Role?.RoleName == "Admin")
            throw new InvalidOperationException("Admin accounts cannot be permanently deleted.");

        var hasBusinessData =
            await _context.Horses.AnyAsync(h => h.OwnerId == id) ||
            await _context.Invitations.AnyAsync(i => i.OwnerId == id || i.JockeyId == id) ||
            await _context.RaceRegistrations.AnyAsync(r => r.OwnerId == id || r.ReviewedBy == id) ||
            await _context.RaceParticipants.AnyAsync(p => p.JockeyId == id) ||
            await _context.Leaderboards.AnyAsync(l => l.JockeyId == id) ||
            await _context.RefereeAssignments.AnyAsync(a => a.RefereeId == id) ||
            await _context.Violations.AnyAsync(v => v.RefereeId == id) ||
            await _context.Predictions.AnyAsync(p => p.SpectatorId == id) ||
            await _context.PointTransactions.AnyAsync(t => t.SpectatorId == id) ||
            await _context.RewardTransactions.AnyAsync(t => t.SpectatorId == id) ||
            await _context.TopupTransactions.AnyAsync(t => t.SpectatorId == id) ||
            await _context.RaceComments.AnyAsync(c => c.UserId == id) ||
            await _context.HorseVerifications.AnyAsync(v => v.VerifiedBy == id);

        if (hasBusinessData)
            throw new InvalidOperationException(
                "This user has related racing or transaction data and cannot be permanently deleted. Deactivate the account instead.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var refreshTokens = await _context.RefreshTokens
                .Where(t => t.UserId == id)
                .ToListAsync();
            _context.RefreshTokens.RemoveRange(refreshTokens);

            var jockeyProfile = await _context.JockeyProfiles.FindAsync(id);
            if (jockeyProfile != null)
                _context.JockeyProfiles.Remove(jockeyProfile);

            var ownerProfile = await _context.OwnerProfiles.FindAsync(id);
            if (ownerProfile != null)
                _context.OwnerProfiles.Remove(ownerProfile);

            var refereeProfile = await _context.RefereeProfiles.FindAsync(id);
            if (refereeProfile != null)
                _context.RefereeProfiles.Remove(refereeProfile);

            var spectatorProfile = await _context.SpectatorProfiles.FindAsync(id);
            if (spectatorProfile != null)
                _context.SpectatorProfiles.Remove(spectatorProfile);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw new InvalidOperationException(
                "This user still has related data and cannot be permanently deleted. Deactivate the account instead.");
        }

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

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            bool phoneExists = await _context.AdminProfiles.AnyAsync(p => p.Phone == request.Phone && p.UserId != id) ||
                               await _context.JockeyProfiles.AnyAsync(p => (p.Phone == request.Phone || p.PendingPhone == request.Phone) && p.UserId != id) ||
                               await _context.OwnerProfiles.AnyAsync(p => p.Phone == request.Phone && p.UserId != id) ||
                               await _context.RefereeProfiles.AnyAsync(p => p.Phone == request.Phone && p.UserId != id) ||
                               await _context.SpectatorProfiles.AnyAsync(p => p.Phone == request.Phone && p.UserId != id);
            
            if (phoneExists)
            {
                throw new System.Exception("Phone number is already in use by another account.");
            }
        }

        user.FullName = request.FullName;
        user.Email = request.Email;

        var role = await _context.Roles.SingleOrDefaultAsync(r => r.RoleName == request.Role);
        if (role != null)
        {
            if (user.RoleId != role.RoleId)
            {
                user.RoleId = role.RoleId;
                // Save the role change first so the SQL trigger can run and update the database profile tables
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    var msg = ex.InnerException?.Message ?? ex.Message;
                    throw new System.Exception($"Database error during role change: {msg}");
                }

                // Detach the user from EF Core context and reload from the database to get the fresh profiles created/deleted by the SQL trigger
                _context.Entry(user).State = EntityState.Detached;
                var reloadedUser = await _context.Users
                    .Include(u => u.AdminProfile)
                    .Include(u => u.JockeyProfile)
                    .Include(u => u.OwnerProfile)
                    .Include(u => u.RefereeProfile)
                    .Include(u => u.SpectatorProfile)
                    .FirstOrDefaultAsync(u => u.UserId == id);
                
                if (reloadedUser == null)
                    return false;

                user = reloadedUser;
            }

            if (request.Role == "Jockey")
            {
                var profile = await _context.JockeyProfiles.FirstOrDefaultAsync(p => p.UserId == id);
                if (profile == null)
                {
                    profile = new JockeyProfile { UserId = id };
                    _context.JockeyProfiles.Add(profile);
                }
                profile.Phone = request.Phone;
                if (request.RemoveAvatar) profile.Avatar = null;
                profile.ExperienceYear = request.ExperienceYear;
            }
            else if (request.Role == "HorseOwner")
            {
                var profile = await _context.OwnerProfiles.FirstOrDefaultAsync(p => p.UserId == id);
                if (profile == null)
                {
                    profile = new OwnerProfile { UserId = id };
                    _context.OwnerProfiles.Add(profile);
                }
                profile.Phone = request.Phone;
                if (request.RemoveAvatar) profile.Avatar = null;
            }
            else if (request.Role == "Referee")
            {
                var profile = await _context.RefereeProfiles.FirstOrDefaultAsync(p => p.UserId == id);
                if (profile == null)
                {
                    profile = new RefereeProfile { UserId = id };
                    _context.RefereeProfiles.Add(profile);
                }
                profile.Phone = request.Phone;
                if (request.RemoveAvatar) profile.Avatar = null;
                profile.ExpYears = request.ExpYears;
            }
            else if (request.Role == "Spectator")
            {
                var profile = await _context.SpectatorProfiles.FirstOrDefaultAsync(p => p.UserId == id);
                if (profile == null)
                {
                    profile = new SpectatorProfile { UserId = id };
                    _context.SpectatorProfiles.Add(profile);
                }
                profile.Phone = request.Phone;
                if (request.RemoveAvatar) profile.Avatar = null;
                if (request.TotalPoints.HasValue) profile.TotalPoints = request.TotalPoints.Value;
            }
            else if (request.Role == "Admin")
            {
                var profile = await _context.AdminProfiles.FirstOrDefaultAsync(p => p.UserId == id);
                if (profile == null)
                {
                    profile = new AdminProfile { UserId = id };
                    _context.AdminProfiles.Add(profile);
                }
                profile.Phone = request.Phone;
                if (request.RemoveAvatar) profile.Avatar = null;
            }
        }
        else
        {
            throw new System.Exception("The specified role does not exist.");
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            throw new System.Exception($"Database error saving profile data: {msg}");
        }
        return true;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            throw new System.Exception("Email is already in use.");

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            bool phoneExists = await _context.AdminProfiles.AnyAsync(p => p.Phone == request.Phone) ||
                               await _context.JockeyProfiles.AnyAsync(p => p.Phone == request.Phone || p.PendingPhone == request.Phone) ||
                               await _context.OwnerProfiles.AnyAsync(p => p.Phone == request.Phone) ||
                               await _context.RefereeProfiles.AnyAsync(p => p.Phone == request.Phone) ||
                               await _context.SpectatorProfiles.AnyAsync(p => p.Phone == request.Phone);
            
            if (phoneExists)
            {
                throw new System.Exception("Phone number is already in use by another account.");
            }
        }

        var role = await _context.Roles.SingleOrDefaultAsync(r => r.RoleName == request.Role);
        if (role == null)
            throw new System.Exception("The specified role does not exist.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            RoleId = role.RoleId,
            IsActive = true,
            CreatedAt = System.DateTime.Now,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Default@123")
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Ensure proper Profile gets created immediately for the new user, ignoring trigger if we just want to set fields
        // Actually, the database might have a trigger creating empty profiles. Let's load the user to see if the trigger fired.
        _context.Entry(user).State = EntityState.Detached;
        var reloadedUser = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.AdminProfile)
            .Include(u => u.JockeyProfile)
            .Include(u => u.OwnerProfile)
            .Include(u => u.RefereeProfile)
            .Include(u => u.SpectatorProfile)
            .FirstOrDefaultAsync(u => u.UserId == user.UserId);

        if (reloadedUser == null)
            throw new System.Exception("Error reloading user after creation.");

        user = reloadedUser;

        if (request.Role == "Jockey")
        {
            var profile = user.JockeyProfile ?? new JockeyProfile { UserId = user.UserId };
            if (user.JockeyProfile == null) _context.JockeyProfiles.Add(profile);
            
            profile.Phone = request.Phone;
            profile.ExperienceYear = request.ExperienceYear;
        }
        else if (request.Role == "HorseOwner")
        {
            var profile = user.OwnerProfile ?? new OwnerProfile { UserId = user.UserId };
            if (user.OwnerProfile == null) _context.OwnerProfiles.Add(profile);

            profile.Phone = request.Phone;
        }
        else if (request.Role == "Referee")
        {
            var profile = user.RefereeProfile ?? new RefereeProfile { UserId = user.UserId };
            if (user.RefereeProfile == null) _context.RefereeProfiles.Add(profile);

            profile.Phone = request.Phone;
            profile.ExpYears = request.ExpYears;
        }
        else if (request.Role == "Spectator")
        {
            var profile = user.SpectatorProfile ?? new SpectatorProfile { UserId = user.UserId };
            if (user.SpectatorProfile == null) _context.SpectatorProfiles.Add(profile);

            profile.Phone = request.Phone;
            if (request.TotalPoints.HasValue) profile.TotalPoints = request.TotalPoints.Value;
        }
        else if (request.Role == "Admin")
        {
            var profile = user.AdminProfile ?? new AdminProfile { UserId = user.UserId };
            if (user.AdminProfile == null) _context.AdminProfiles.Add(profile);

            profile.Phone = request.Phone;
        }

        await _context.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.UserId,
            Name = user.FullName,
            Email = user.Email,
            Role = user.Role?.RoleName ?? "Unknown",
            Status = "Active",
            Phone = request.Phone,
            ExperienceYear = request.ExperienceYear,
            ExpYears = request.ExpYears,
            TotalPoints = request.TotalPoints
        };
    }
}
