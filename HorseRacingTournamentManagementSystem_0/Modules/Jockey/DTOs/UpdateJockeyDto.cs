using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs
{
    /// <summary>
    /// DTO dùng cho FR-JCKY-002: Jockey gửi yêu cầu cập nhật thông tin cá nhân.
    ///
    /// === LUỒNG HOẠT ĐỘNG (FR-JCKY-002) ===
    ///   Bước 1 – Jockey đăng nhập vào hệ thống, truy cập trang "Edit Profile".
    ///   Bước 2 – Jockey điền các thông tin muốn cập nhật (phone, avatar, weight, exp).
    ///   Bước 3 – Client gửi request: PUT /api/jockeys/{id}  với body là DTO này.
    ///   Bước 4 – Service lưu dữ liệu pending vào các cột PendingXxx trong DB
    ///             và đặt UpdateStatus = "Pending" để Admin biết có đơn chờ duyệt.
    ///   Bước 5 – Thông tin chính thức chưa thay đổi cho đến khi Admin duyệt (FR-JCKY-004).
    /// </summary>
    public class UpdateJockeyDto
    {
        /// <summary>
        /// Số điện thoại liên hệ mới của jockey (tùy chọn, tối đa 20 ký tự).
        /// </summary>
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }

        /// <summary>
        /// URL ảnh đại diện mới (tùy chọn, tối đa 255 ký tự).
        /// </summary>
        [MaxLength(255, ErrorMessage = "Image URL cannot exceed 255 characters")]
        public string? Avatar { get; set; }

        /// <summary>
        /// Số năm kinh nghiệm đua ngựa (tùy chọn, 0–60 năm).
        /// </summary>
        [Range(0, 60, ErrorMessage = "Experience years must be between 0 and 60")]
        public int? ExperienceYear { get; set; }
    }
}
