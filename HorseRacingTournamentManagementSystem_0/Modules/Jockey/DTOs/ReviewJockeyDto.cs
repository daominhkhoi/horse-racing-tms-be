using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs
{
    /// <summary>
    /// DTO dùng cho FR-JCKY-004: Admin xem xét và xác nhận / từ chối đơn cập nhật thông tin của Jockey.
    ///
    /// === LUỒNG HOẠT ĐỘNG (FR-JCKY-004 – Admin review) ===
    ///   Bước 1 – Admin đăng nhập, vào trang "Pending Jockey Requests".
    ///   Bước 2 – Admin xem thông tin pending (PendingPhone, PendingWeight, ...) mà jockey đã gửi.
    ///   Bước 3 – Admin chọn Approve hoặc Reject, nhập ghi chú (tùy chọn).
    ///   Bước 4 – Client gửi request: PUT /api/jockeys/{id}/review  với body là DTO này.
    ///   Bước 5 – Nếu IsApproved = true:
    ///               → Service copy dữ liệu từ Pending sang cột chính thức.
    ///               → Đặt UpdateStatus = "Approved".
    ///            Nếu IsApproved = false:
    ///               → Xóa dữ liệu pending, đặt UpdateStatus = "Rejected".
    ///   Bước 6 – Jockey có thể xem kết quả duyệt trên trang cá nhân của mình.
    /// </summary>
    public class ReviewJockeyDto
    {
        /// <summary>
        /// true  → Admin DUYỆT yêu cầu: dữ liệu pending được áp dụng vào hồ sơ chính thức.
        /// false → Admin TỪ CHỐI yêu cầu: dữ liệu pending bị hủy bỏ.
        /// </summary>
        [Required(ErrorMessage = "IsApproved là bắt buộc (true = duyệt / false = từ chối)")]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Ghi chú của Admin (lý do từ chối hoặc nhận xét khi duyệt) – tùy chọn, tối đa 500 ký tự.
        /// </summary>
        [MaxLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? Notes { get; set; }

        /// <summary>
        /// UserId của Admin đang thực hiện thao tác duyệt.
        /// Dùng để lưu audit trail: ai đã duyệt / từ chối đơn này.
        /// </summary>
        [Required(ErrorMessage = "ReviewedBy (UserId của Admin) là bắt buộc")]
        public int ReviewedBy { get; set; }
    }
}
