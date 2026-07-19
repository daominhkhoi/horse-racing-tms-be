using System.Security.Claims;
using HorseRacingTournamentManagementSystem_0.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorseRacingTournamentManagementSystem_0.Modules.Predictions.Controllers;

[ApiController]
[Route("api/points")]
[Authorize]
public class PointsController : ControllerBase
{
    private readonly HorseRacingDbContext _context;

    public PointsController(HorseRacingDbContext context) => _context = context;

    [HttpGet("history")]
    [Authorize(Roles = "Spectator")]
    public async Task<IActionResult> History([FromQuery] int limit = 50)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        limit = Math.Clamp(limit, 1, 100);

        var items = await _context.PointTransactions
            .Where(t => t.SpectatorId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .Select(t => new
            {
                t.TransactionId,
                t.PredictionId,
                t.Amount,
                t.TransactionType,
                t.Description,
                t.CreatedAt
            })
            .ToListAsync();

        return Ok(new { data = items });
    }

    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> Leaderboard([FromQuery] int limit = 20)
    {
        limit = Math.Clamp(limit, 1, 100);
        var rows = await _context.SpectatorProfiles
            .Select(s => new
            {
                spectatorId = s.UserId,
                name = s.User.FullName,
                avatar = s.Avatar,
                totalPoints = s.TotalPoints ?? 0,
                bettingProfit = s.PointTransactions
                    .Where(t => t.TransactionType == "BetPlaced" || t.TransactionType == "BetWon" || t.TransactionType == "BetRefund")
                    .Sum(t => (double?)t.Amount) ?? 0,
                wins = s.Predictions.Count(p => p.Status == "Won")
            })
            .OrderByDescending(s => s.bettingProfit)
            .ThenByDescending(s => s.wins)
            .Take(limit)
            .ToListAsync();

        return Ok(new { data = rows });
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out userId);
    }
}
