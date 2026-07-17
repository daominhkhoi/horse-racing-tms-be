using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Horses.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Horses.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Horses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorsesController : ControllerBase
    {
        private readonly IHorseService _horseService;

        public HorsesController(IHorseService horseService)
        {
            _horseService = horseService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterHorse([FromBody] CreateHorseDto createHorseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var horse = await _horseService.RegisterHorseAsync(createHorseDto);
                return Ok(new { message = "Horse registered successfully!", data = horse });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error registering horse", error = ex.Message });
            }
        }
        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetHorsesByOwner(int ownerId)
        {
            try
            {
                var horses = await _horseService.GetHorsesByOwnerAsync(ownerId);
                return Ok(new { message = "Horse list retrieved successfully!", data = horses });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving horse list", error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllHorses()
        {
            try
            {
                var horses = await _horseService.GetAllHorsesAsync();
                return Ok(new { message = "Horse list retrieved successfully!", data = horses });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving horse list", error = ex.Message });
            }
        }

        [HttpPut("{id}/verify")]
        public async Task<IActionResult> VerifyHorse(int id, [FromBody] VerifyHorseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var horse = await _horseService.VerifyHorseAsync(id, dto);
                return Ok(new { message = "Horse application verified successfully!", data = horse });
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error verifying horse application", error = ex.Message, innerError = ex.InnerException?.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHorse(int id)
        {
            try
            {
                var result = await _horseService.DeleteHorseAsync(id);
                if (!result) return NotFound(new { message = "Horse not found" });
                return Ok(new { message = "Horse retired successfully!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error retiring horse", error = ex.Message });
            }
        }

        [HttpPut("{id}/suspend")]
        public async Task<IActionResult> SuspendHorse(int id)
        {
            try
            {
                var result = await _horseService.UpdateHorseStatusAsync(id, "Suspended");
                if (!result) return NotFound(new { message = "Horse not found" });
                return Ok(new { message = "Horse suspended successfully!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error suspending horse", error = ex.Message });
            }
        }

        [HttpPut("{id}/reinstate")]
        public async Task<IActionResult> ReinstateHorse(int id)
        {
            try
            {
                var result = await _horseService.UpdateHorseStatusAsync(id, "Approved");
                if (!result) return NotFound(new { message = "Horse not found" });
                return Ok(new { message = "Horse reinstated successfully!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error reinstating horse", error = ex.Message });
            }
        }

        [HttpPost("{id}/update-request")]
        public async Task<IActionResult> RequestUpdateHorse(int id, [FromBody] UpdateHorseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _horseService.RequestUpdateHorseAsync(id, dto);
                if (!result) return NotFound(new { message = "Horse not found" });
                return Ok(new { message = "Horse update request submitted successfully, pending verification!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error submitting horse update request", error = ex.Message });
            }
        }

        [HttpPut("{id}/approve-update")]
        public async Task<IActionResult> ApproveUpdateHorse(int id, [FromBody] VerifyHorseDto dto)
        {
            try
            {
                var result = await _horseService.ApproveUpdateHorseAsync(id, dto.VerifiedBy, dto.Notes ?? "");
                if (!result) return NotFound(new { message = "Update request or horse not found" });
                return Ok(new { message = "Horse update request approved successfully!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error approving horse update", error = ex.Message });
            }
        }

        [HttpPut("{id}/reject-update")]
        public async Task<IActionResult> RejectUpdateHorse(int id, [FromBody] VerifyHorseDto dto)
        {
            try
            {
                var result = await _horseService.RejectUpdateHorseAsync(id, dto.VerifiedBy, dto.Notes ?? "");
                if (!result) return NotFound(new { message = "Update request or horse not found" });
                return Ok(new { message = "Horse update request rejected successfully!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Error rejecting horse update", error = ex.Message });
            }
        }
    }
}
