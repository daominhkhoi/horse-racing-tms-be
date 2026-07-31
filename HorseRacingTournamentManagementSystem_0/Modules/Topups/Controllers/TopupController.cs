using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HorseRacingTournamentManagementSystem_0.Modules.Topups.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Topups.Services;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using Microsoft.EntityFrameworkCore;

namespace HorseRacingTournamentManagementSystem_0.Modules.Topups.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TopupController : ControllerBase
{
    private readonly IVNPayService _vnPayService;
    private readonly HorseRacingDbContext _context;

    public TopupController(IVNPayService vnPayService, HorseRacingDbContext context)
    {
        _vnPayService = vnPayService;
        _context = context;
    }

    [Authorize(Roles = "Spectator")]
    [HttpPost("vnpay/create-url")]
    public IActionResult CreateUrl([FromBody] VNPayTopupRequestDto request)
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int spectatorId))
        {
            return Unauthorized("Invalid token or user ID.");
        }

        string url = _vnPayService.CreatePaymentUrl(HttpContext, request.Amount, spectatorId);
        return Ok(new { url });
    }

    [HttpGet("vnpay/ipn")]
    public async Task<IActionResult> VnpayIpn()
    {
        var result = await _vnPayService.ProcessIpn(Request.Query);

        if (result == "Success" || result == "Transaction already processed")
        {
            return Ok(new { RspCode = "00", Message = result });
        }
        else
        {
            return Ok(new { RspCode = "99", Message = result });
        }
    }

    [Authorize(Roles = "Spectator")]
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequestDto request)
    {
        if (request.Amount < 10)
        {
            return BadRequest(new { message = "Minimum withdrawal amount is 10 PTS (10,000 VND)." });
        }

        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int spectatorId))
        {
            return Unauthorized(new { message = "Invalid token or user ID." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var spectator = await _context.SpectatorProfiles.FindAsync(spectatorId);
            if (spectator == null)
            {
                return NotFound(new { message = "Spectator not found." });
            }

            var currentPoints = spectator.TotalPoints ?? 0;
            if (request.Amount > currentPoints)
            {
                return BadRequest(new { message = "Insufficient points balance." });
            }

            spectator.TotalPoints = currentPoints - request.Amount;

            double taxAmount = request.Amount * 0.05;
            double payoutAmount = request.Amount - taxAmount;

            var pointTransaction = new PointTransaction
            {
                SpectatorId = spectatorId,
                Amount = -request.Amount, // Using negative to denote withdrawal/deduction
                TransactionType = "Withdrawal",
                Description = $"Withdrawal of {request.Amount} PTS to {request.BankName} - {request.AccountNumber}. Fee: {taxAmount} PTS. Payout: {payoutAmount * 1000:N0} VND."
            };

            _context.PointTransactions.Add(pointTransaction);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new { message = "Withdrawal request processed successfully.", newBalance = spectator.TotalPoints });
        }
        catch (System.Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "An error occurred while processing withdrawal.", error = ex.Message });
        }
    }
}
