using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HorseRacingTournamentManagementSystem_0.Modules.Topups.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Topups.Services;

namespace HorseRacingTournamentManagementSystem_0.Modules.Topups.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TopupController : ControllerBase
{
    private readonly IVNPayService _vnPayService;

    public TopupController(IVNPayService vnPayService)
    {
        _vnPayService = vnPayService;
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
}
