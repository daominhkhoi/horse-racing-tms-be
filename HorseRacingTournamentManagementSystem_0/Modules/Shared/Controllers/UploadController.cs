using HorseRacingTournamentManagementSystem_0.Modules.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Shared.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UploadController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;

    public UploadController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("Image")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] bool banner = false)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Message = "No file uploaded." });
        }

        var resultUrl = await _cloudinaryService.UploadImageAsync(file, banner);

        if (resultUrl == null)
        {
            return StatusCode(500, new { Message = "Failed to upload image." });
        }

        return Ok(new { Url = resultUrl });
    }
}
