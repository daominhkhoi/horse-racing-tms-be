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
                return Ok(new { message = "Đăng ký ngựa thành công!", data = horse });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi đăng ký ngựa", error = ex.Message });
            }
        }
        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetHorsesByOwner(int ownerId)
        {
            try
            {
                var horses = await _horseService.GetHorsesByOwnerAsync(ownerId);
                return Ok(new { message = "Lấy danh sách ngựa thành công!", data = horses });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách ngựa", error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllHorses()
        {
            try
            {
                var horses = await _horseService.GetAllHorsesAsync();
                return Ok(new { message = "Lấy danh sách ngựa thành công!", data = horses });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách ngựa", error = ex.Message });
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
                return Ok(new { message = "Duyệt đơn ngựa thành công!", data = horse });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi duyệt đơn ngựa", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHorse(int id)
        {
            try
            {
                var result = await _horseService.DeleteHorseAsync(id);
                if (!result) return NotFound(new { message = "Không tìm thấy ngựa" });
                return Ok(new { message = "Retire ngựa thành công!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi retire ngựa", error = ex.Message });
            }
        }

        [HttpPut("{id}/suspend")]
        public async Task<IActionResult> SuspendHorse(int id)
        {
            try
            {
                var result = await _horseService.UpdateHorseStatusAsync(id, "Suspended");
                if (!result) return NotFound(new { message = "Không tìm thấy ngựa" });
                return Ok(new { message = "Đình chỉ ngựa thành công!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi đình chỉ ngựa", error = ex.Message });
            }
        }

        [HttpPut("{id}/reinstate")]
        public async Task<IActionResult> ReinstateHorse(int id)
        {
            try
            {
                var result = await _horseService.UpdateHorseStatusAsync(id, "Approved");
                if (!result) return NotFound(new { message = "Không tìm thấy ngựa" });
                return Ok(new { message = "Khôi phục ngựa thành công!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi khôi phục ngựa", error = ex.Message });
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
                if (!result) return NotFound(new { message = "Không tìm thấy ngựa" });
                return Ok(new { message = "Gửi yêu cầu cập nhật ngựa thành công, đang chờ duyệt!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gửi yêu cầu cập nhật ngựa", error = ex.Message });
            }
        }

        [HttpPut("{id}/approve-update")]
        public async Task<IActionResult> ApproveUpdateHorse(int id, [FromBody] VerifyHorseDto dto)
        {
            try
            {
                var result = await _horseService.ApproveUpdateHorseAsync(id, dto.VerifiedBy, dto.Notes ?? "");
                if (!result) return NotFound(new { message = "Không tìm thấy yêu cầu cập nhật hoặc ngựa" });
                return Ok(new { message = "Duyệt yêu cầu cập nhật ngựa thành công!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi duyệt cập nhật ngựa", error = ex.Message });
            }
        }

        [HttpPut("{id}/reject-update")]
        public async Task<IActionResult> RejectUpdateHorse(int id, [FromBody] VerifyHorseDto dto)
        {
            try
            {
                var result = await _horseService.RejectUpdateHorseAsync(id, dto.VerifiedBy, dto.Notes ?? "");
                if (!result) return NotFound(new { message = "Không tìm thấy yêu cầu cập nhật hoặc ngựa" });
                return Ok(new { message = "Từ chối yêu cầu cập nhật ngựa thành công!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi từ chối cập nhật ngựa", error = ex.Message });
            }
        }
    }
}
