using System.Collections.Generic;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.Interfaces
{
    /// <summary>
    /// Interface định nghĩa hợp đồng (contract) cho JockeyService.
    /// Mọi logic nghiệp vụ liên quan đến Jockey phải đi qua interface này
    /// để đảm bảo tính mô-đun hóa và dễ kiểm thử (unit test / mock).
    ///
    /// Bao gồm 3 chức năng chính theo requirement:
    ///   • FR-JCKY-002 → RequestUpdateProfileAsync  (Jockey gửi đơn cập nhật)
    ///   • FR-JCKY-004 → ReviewUpdateRequestAsync   (Admin duyệt / từ chối đơn)
    ///   • FR-JCKY-004 → GetAllJockeysPublicAsync   (Hiển thị danh sách Jockey công khai)
    /// </summary>
    public interface IJockeyService
    {
        /// <summary>
        /// [FR-JCKY-002] Jockey gửi yêu cầu cập nhật thông tin cá nhân.
        ///
        /// LUỒNG: Jockey điền form → gọi API PUT /api/jockeys/{id}
        ///        → Service lưu dữ liệu mới vào trường Pending trong DB
        ///        → Đặt UpdateStatus = "Pending" chờ Admin duyệt.
        /// </summary>
        /// <param name="jockeyId">UserId của jockey cần cập nhật.</param>
        /// <param name="dto">Dữ liệu mới mà jockey muốn cập nhật.</param>
        /// <returns>true nếu lưu thành công; false nếu không tìm thấy jockey.</returns>
        Task<bool> RequestUpdateProfileAsync(int jockeyId, UpdateJockeyDto dto);

        /// <summary>
        /// [FR-JCKY-004] Admin xem xét và xác nhận / từ chối đơn cập nhật của Jockey.
        ///
        /// LUỒNG: Admin chọn Approve / Reject → gọi API PUT /api/jockeys/{id}/review
        ///        → Nếu Approve: copy Pending → chính thức, UpdateStatus = "Approved"
        ///        → Nếu Reject : xóa Pending , UpdateStatus = "Rejected"
        /// </summary>
        /// <param name="jockeyId">UserId của jockey cần được xem xét.</param>
        /// <param name="dto">Quyết định của Admin (duyệt / từ chối + ghi chú).</param>
        /// <returns>true nếu xử lý thành công; false nếu không có đơn pending hoặc không tìm thấy.</returns>
        Task<bool> ReviewUpdateRequestAsync(int jockeyId, ReviewJockeyDto dto);

        /// <summary>
        /// [FR-JCKY-004] Trả về danh sách công khai tất cả Jockey trong hệ thống.
        ///
        /// LUỒNG: Bất kỳ người dùng nào → gọi API GET /api/jockeys
        ///        → Service truy vấn DB, lấy Jockey_Profiles kèm thông tin User
        ///        → Trả về danh sách (không cần đăng nhập).
        /// </summary>
        /// <returns>Danh sách tất cả JockeyProfile cùng thông tin User đi kèm.</returns>
        Task<IEnumerable<JockeyProfile>> GetAllJockeysPublicAsync();
    }
}
