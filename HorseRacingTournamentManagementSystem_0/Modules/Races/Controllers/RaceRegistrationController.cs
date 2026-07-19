using System.Security.Claims;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HorseRacingTournamentManagementSystem_0.Database;
using Microsoft.EntityFrameworkCore;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Controllers;

[Route("api/races")]
[ApiController]
[Authorize]
public class RaceRegistrationController : ControllerBase
{
    private readonly IRaceRegistrationService _service;
    private readonly HorseRacingDbContext _context;
    public RaceRegistrationController(IRaceRegistrationService service, HorseRacingDbContext context)
    {
        _service = service;
        _context = context;
    }

    private int CurrentUserId()
    {
        var value = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var id) ? id : throw new Exception("User ID not found in token.");
    }

    [HttpPost("{raceId}/registrations")]
    [Authorize(Roles = "HorseOwner")]
    public Task<IActionResult> RegisterHorse(int raceId, RegisterHorseRequest request) => Execute(async () => Ok(await _service.RegisterHorseAsync(CurrentUserId(), raceId, request)));

    [HttpGet("{raceId}/available-horses")]
    [Authorize(Roles = "HorseOwner")]
    public Task<IActionResult> AvailableHorses(int raceId) => Execute(async () => Ok(await _service.GetAvailableHorsesAsync(CurrentUserId(), raceId)));

    [HttpGet("{raceId}/registrations")]
    [Authorize(Roles = "Admin,Referee,HorseOwner")]
    public Task<IActionResult> Registrations(int raceId, [FromQuery] string? status) => Execute(async () => Ok(await _service.GetRegistrationsAsync(raceId, status)));

    [HttpGet("registrations/my")]
    [Authorize(Roles = "HorseOwner")]
    public Task<IActionResult> MyRegistrations() => Execute(async () => Ok(await _service.GetMyRegistrationsAsync(CurrentUserId())));

    [HttpPut("registrations/{id}/approve")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Approve(int id, RegistrationReviewRequest request) => Execute(async () => Ok(await _service.ReviewRegistrationAsync(CurrentUserId(), id, true, request.Reason)));

    [HttpPut("registrations/{id}/reject")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Reject(int id, RegistrationReviewRequest request) => Execute(async () => Ok(await _service.ReviewRegistrationAsync(CurrentUserId(), id, false, request.Reason)));

    [HttpGet("{raceId}/registration-summary")]
    public Task<IActionResult> Summary(int raceId) => Execute(async () => Ok(await _service.GetSummaryAsync(raceId)));

    [HttpPut("{raceId}/open-registration")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Open(int raceId) => Execute(async () => { await _service.SetRegistrationStatusAsync(raceId, "Open Registration"); return Ok(new { message = "Registration opened." }); });

    [HttpPut("{raceId}/close-registration")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Close(int raceId) => Execute(async () => { await _service.SetRegistrationStatusAsync(raceId, "Registration Closed"); return Ok(new { message = "Registration closed." }); });

    [HttpPost("{raceId}/start")]
    [Authorize(Roles = "Referee")]
    public Task<IActionResult> Start(int raceId) => Execute(async () =>
    {
        var refereeId = CurrentUserId();
        var isAssigned = await _context.RefereeAssignments
            .AnyAsync(a => a.RaceId == raceId && a.RefereeId == refereeId);
        if (!isAssigned)
            return new ForbidResult();

        await _service.StartRaceAsync(raceId);
        return Ok(new { message = "Race started." });
    });

    private static async Task<IActionResult> Execute(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (Exception ex) { return new BadRequestObjectResult(new { message = ex.InnerException?.Message ?? ex.Message }); }
    }
}
