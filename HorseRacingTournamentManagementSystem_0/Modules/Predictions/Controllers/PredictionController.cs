using HorseRacingTournamentManagementSystem_0.Modules.Predictions.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Predictions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HorseRacingTournamentManagementSystem_0.Modules.Predictions.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Spectator")]
public class PredictionController : ControllerBase
{
    private readonly IPredictionService _predictionService;

    public PredictionController(IPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    [HttpPost("bet")]
    public async Task<IActionResult> PlaceBet([FromBody] BetRequestDto request)
    {
        if (request.BetPoints <= 0) return BadRequest(new { Message = "Bet points must be greater than zero." });

        var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var result = await _predictionService.PlaceBetAsync(userId, request);
        if (result == "Bet placed successfully.")
            return Ok(new { Message = result });

        return BadRequest(new { Message = result });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBet(int id)
    {
        var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var result = await _predictionService.CancelBetAsync(userId, id);
        if (result.Contains("successfully"))
            return Ok(new { Message = result });

        return BadRequest(new { Message = result });
    }

    [HttpGet("my-bets")]
    public async Task<IActionResult> GetMyBets()
    {
        var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var predictions = await _predictionService.GetMyPredictionsAsync(userId);
        return Ok(new { data = predictions });
    }
}
