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
        [MaxLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string? Phone { get; set; }

        /// <summary>
        /// URL ảnh đại diện mới (tùy chọn, tối đa 255 ký tự).
        /// </summary>
        [MaxLength(255, ErrorMessage = "URL ảnh không được quá 255 ký tự")]
        public string? Avatar { get; set; }

        /// <summary>
        /// Cân nặng hiện tại (kg). Thông tin quan trọng ảnh hưởng đến
        /// điều kiện tham gia thi đấu – bắt buộc Admin phải xác nhận trước khi áp dụng.
        /// </summary>
        [Range(30, 150, ErrorMessage = "Cân nặng phải nằm trong khoảng 30 – 150 kg")]
        public double? Weight { get; set; }

        /// <summary>
        /// Số năm kinh nghiệm đua ngựa (tùy chọn, 0–60 năm).
        /// </summary>
        [Range(0, 60, ErrorMessage = "Số năm kinh nghiệm phải từ 0 đến 60")]
        public int? ExperienceYear { get; set; }
    }
}
