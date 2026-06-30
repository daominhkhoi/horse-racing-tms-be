using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.Controllers
{
    /// <summary>
    /// Controller xử lý các HTTP request liên quan đến Jockey.
    /// Base route: /api/jockeys
    ///
    /// Danh sách endpoints:
    ///   GET  /api/jockeys           → Danh sách công khai Jockey (AllowAnonymous)
    ///   PUT  /api/jockeys/{id}      → Jockey gửi yêu cầu cập nhật thông tin (Role: Jockey)
    ///   PUT  /api/jockeys/{id}/review → Admin duyệt / từ chối đơn cập nhật (Role: Admin)
    /// </summary>
    [Route("api/jockeys")]
    [ApiController]
    public class JockeysController : ControllerBase
    {
        private readonly IJockeyService _jockeyService;

        /// <summary>
        /// Inject IJockeyService thông qua DI container của ASP.NET Core.
        /// </summary>
        public JockeysController(IJockeyService jockeyService)
        {
            _jockeyService = jockeyService;
        }

        // =====================================================================
        // FR-JCKY-004 (Public): GET /api/jockeys
        // Hiển thị danh sách công khai tất cả Jockey – không cần đăng nhập
        // =====================================================================

        /// <summary>
        /// [FR-JCKY-004] Trả về danh sách toàn bộ Jockey trong hệ thống (dành cho tất cả người dùng).
        ///
        /// === LUỒNG HTTP ===
        ///   Client → GET /api/jockeys
        ///   Controller → gọi _jockeyService.GetAllJockeysPublicAsync()
        ///   Service    → truy vấn DB, Include User, sắp xếp mới nhất trước
        ///   Controller → trả 200 OK kèm danh sách JSON
        ///
        /// === PHÂN QUYỀN ===
        ///   [AllowAnonymous]: bất kỳ ai cũng có thể gọi, kể cả chưa đăng nhập.
        ///   Phù hợp với trang giới thiệu Jockey công khai (landing page, SEO...).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllJockeys()
        {
            try
            {
                var jockeys = await _jockeyService.GetAllJockeysPublicAsync();
                return Ok(new
                {
                    message = "Jockey list retrieved successfully!",
                    data = jockeys
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error retrieving jockey list",
                    error = ex.Message
                });
            }
        }

        // =====================================================================
        // FR-JCKY-002: PUT /api/jockeys/{id}
        // Jockey gửi đơn yêu cầu cập nhật thông tin cá nhân
        // =====================================================================

        /// <summary>
        /// [FR-JCKY-002] Jockey tự gửi yêu cầu cập nhật thông tin cá nhân (weight, phone, avatar...).
        ///
        /// === LUỒNG HTTP ===
        ///   Client → PUT /api/jockeys/{id}  Body: { phone, avatar, weight, experienceYear }
        ///   Controller → validate ModelState
        ///   Controller → gọi _jockeyService.RequestUpdateProfileAsync(id, dto)
        ///   Service    → lưu dữ liệu vào cột Pending, UpdateStatus = "Pending"
        ///   Controller → trả 200 OK nếu thành công / 404 nếu không tìm thấy jockey
        ///
        /// === PHÂN QUYỀN ===
        ///   [Authorize(Roles = "Jockey")]: chỉ tài khoản có role Jockey mới được gọi.
        ///   Jockey chỉ được cập nhật chính profile của mình (id = UserId của họ).
        /// </summary>
        /// <param name="id">UserId của Jockey cần cập nhật.</param>
        /// <param name="dto">Dữ liệu mới mà Jockey muốn thay đổi.</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Jockey")]
        public async Task<IActionResult> RequestUpdateProfile(int id, [FromBody] UpdateJockeyDto dto)
        {
            // Validate các DataAnnotation trên DTO (Range, MaxLength...)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _jockeyService.RequestUpdateProfileAsync(id, dto);

                if (!result)
                    return NotFound(new { message = "Jockey profile not found for this ID." });

                return Ok(new
                {
                    message = "Update request submitted successfully! Please wait for Admin approval."
                });
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("Phone number"))
                {
                    return BadRequest(new { message = ex.Message });
                }
                return StatusCode(500, new
                {
                    message = "Error submitting jockey update request",
                    error = ex.Message
                });
            }
        }

        // =====================================================================
        // FR-JCKY-004 (Admin): PUT /api/jockeys/{id}/review
        // Admin duyệt hoặc từ chối đơn cập nhật thông tin của Jockey
        // =====================================================================

        /// <summary>
        /// [FR-JCKY-004] Admin xem xét và xác nhận / từ chối đơn cập nhật thông tin của Jockey.
        ///
        /// === LUỒNG HTTP ===
        ///   Client → PUT /api/jockeys/{id}/review  Body: { isApproved, notes, reviewedBy }
        ///   Controller → validate ModelState
        ///   Controller → gọi _jockeyService.ReviewUpdateRequestAsync(id, dto)
        ///   Service    → tìm profile có UpdateStatus = "Pending"
        ///              → Nếu Approve: copy Pending → chính thức, UpdateStatus = "Approved"
        ///              → Nếu Reject : xóa Pending,               UpdateStatus = "Rejected"
        ///   Controller → trả 200 OK kèm thông báo kết quả / 404 nếu không có đơn pending
        ///
        /// === PHÂN QUYỀN ===
        ///   [Authorize(Roles = "Admin")]: chỉ tài khoản Admin mới được gọi endpoint này.
        /// </summary>
        /// <param name="id">UserId của Jockey cần được Admin xem xét.</param>
        /// <param name="dto">Quyết định của Admin (duyệt/từ chối + ghi chú).</param>
        [HttpPut("{id}/review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewUpdateRequest(int id, [FromBody] ReviewJockeyDto dto)
        {
            // Validate các DataAnnotation trên DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _jockeyService.ReviewUpdateRequestAsync(id, dto);

                if (!result)
                    return NotFound(new
                    {
                        message = "No pending update request found for this jockey."
                    });

                // Trả về thông báo phù hợp theo quyết định của Admin
                var action = dto.IsApproved ? "approved" : "rejected";
                return Ok(new
                {
                    message = $"Jockey update request {action} successfully!"
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error processing jockey update review",
                    error = ex.Message
                });
            }
        }
    }
}
