using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HorseRacingTournamentManagementSystem_0.Modules.Shared.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly HorseRacingDbContext _context;

    public ProfileController(HorseRacingDbContext context)
    {
        _context = context;
    }

    [HttpGet("Me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.AdminProfile)
            .Include(u => u.OwnerProfile)
            .Include(u => u.JockeyProfile)
            .Include(u => u.RefereeProfile)
            .Include(u => u.SpectatorProfile)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        var roleName = user.Role.RoleName;
        string? phone = null;
        string? avatarUrl = null;

        if (user.AdminProfile != null) { phone = user.AdminProfile.Phone; avatarUrl = user.AdminProfile.Avatar; }
        else if (user.OwnerProfile != null) { phone = user.OwnerProfile.Phone; avatarUrl = user.OwnerProfile.Avatar; }
        else if (user.JockeyProfile != null) { phone = user.JockeyProfile.Phone; avatarUrl = user.JockeyProfile.Avatar; }
        else if (user.RefereeProfile != null) { phone = user.RefereeProfile.Phone; avatarUrl = user.RefereeProfile.Avatar; }
        else if (user.SpectatorProfile != null) { phone = user.SpectatorProfile.Phone; avatarUrl = user.SpectatorProfile.Avatar; }

        var dto = new UserProfileDto
        {
            FullName = user.FullName,
            Email = user.Email,
            Role = roleName,
            Phone = phone,
            AvatarUrl = avatarUrl,
            JoinedDate = user.CreatedAt,
            IsActive = user.IsActive ?? true
        };

        return Ok(dto);
    }

    [HttpPut("Me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
    {
        var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.AdminProfile)
            .Include(u => u.OwnerProfile)
            .Include(u => u.JockeyProfile)
            .Include(u => u.RefereeProfile)
            .Include(u => u.SpectatorProfile)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        user.FullName = dto.FullName;
        if (!string.IsNullOrEmpty(dto.Email))
        {
            user.Email = dto.Email;
        }

        // Update corresponding profile table based on RoleId
        // Assuming roles: 1=Admin, 2=Owner, 3=Jockey, 4=Referee, 5=Spectator (or similar, we check RoleName)
        var roleName = user.Role.RoleName;

        switch (roleName)
        {
            case "Admin":
                if (user.AdminProfile == null)
                {
                    user.AdminProfile = new AdminProfile { UserId = userId };
                    _context.AdminProfiles.Add(user.AdminProfile);
                }
                user.AdminProfile.Phone = dto.Phone;
                user.AdminProfile.Avatar = dto.AvatarUrl;
                break;
            case "HorseOwner":
                if (user.OwnerProfile == null)
                {
                    user.OwnerProfile = new OwnerProfile { UserId = userId };
                    _context.OwnerProfiles.Add(user.OwnerProfile);
                }
                user.OwnerProfile.Phone = dto.Phone;
                user.OwnerProfile.Avatar = dto.AvatarUrl;
                break;
            case "Jockey":
                if (user.JockeyProfile == null)
                {
                    user.JockeyProfile = new JockeyProfile { UserId = userId };
                    _context.JockeyProfiles.Add(user.JockeyProfile);
                }
                user.JockeyProfile.Phone = dto.Phone;
                user.JockeyProfile.Avatar = dto.AvatarUrl;
                break;
            case "Referee":
                if (user.RefereeProfile == null)
                {
                    user.RefereeProfile = new RefereeProfile { UserId = userId };
                    _context.RefereeProfiles.Add(user.RefereeProfile);
                }
                user.RefereeProfile.Phone = dto.Phone;
                user.RefereeProfile.Avatar = dto.AvatarUrl;
                break;
            case "Spectator":
            default:
                if (user.SpectatorProfile == null)
                {
                    user.SpectatorProfile = new SpectatorProfile { UserId = userId };
                    _context.SpectatorProfiles.Add(user.SpectatorProfile);
                }
                user.SpectatorProfile.Phone = dto.Phone;
                user.SpectatorProfile.Avatar = dto.AvatarUrl;
                break;
        }

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Profile updated successfully." });
    }
}
