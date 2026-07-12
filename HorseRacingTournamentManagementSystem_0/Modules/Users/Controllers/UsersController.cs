using HorseRacingTournamentManagementSystem_0.Modules.Common.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Users.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Users.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string search = "", 
        [FromQuery] string role = "All", 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _userService.GetUsersAsync(search, role, page, pageSize);

        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var success = await _userService.ToggleUserStatusAsync(id);
        if (!success)
            return NotFound(new { message = "User not found." });

        return Ok(new { message = "User status updated successfully." });
    }

    [HttpPut("{id}/update")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            var success = await _userService.UpdateUserAsync(id, request);
            if (!success)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = "User updated successfully." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var newUser = await _userService.CreateUserAsync(request);
            return Ok(newUser);
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = "User deleted permanently." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
