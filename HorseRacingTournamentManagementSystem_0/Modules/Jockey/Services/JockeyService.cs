using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.Services
{
    /// <summary>
    /// Triển khai IJockeyService – chứa toàn bộ logic nghiệp vụ liên quan đến Jockey.
    ///
    /// Dependency: HorseRacingDbContext (inject qua constructor).
    /// Đăng ký trong Program.cs:
    ///     builder.Services.AddScoped&lt;IJockeyService, JockeyService&gt;();
    /// </summary>
    public class JockeyService : IJockeyService
    {
        private readonly HorseRacingDbContext _context;

        /// <summary>
        /// Constructor nhận DbContext thông qua Dependency Injection (DI).
        /// ASP.NET Core tự động inject instance đúng scope (Scoped) khi xử lý mỗi HTTP request.
        /// </summary>
        public JockeyService(HorseRacingDbContext context)
        {
            _context = context;
        }

        // =====================================================================
        // FR-JCKY-002: Jockey gửi yêu cầu cập nhật thông tin cá nhân
        // Endpoint: PUT /api/jockeys/{id}
        // =====================================================================

        /// <summary>
        /// Lưu thông tin cập nhật của Jockey vào trạng thái "Pending" để chờ Admin duyệt.
        ///
        /// === LUỒNG HOẠT ĐỘNG ===
        ///   1. Tìm JockeyProfile theo jockeyId trong bảng Jockey_Profiles.
        ///   2. Kiểm tra xem Jockey có tồn tại không → nếu không, trả về false.
        ///   3. Ghi dữ liệu mới từ DTO vào các cột "Pending" (PendingPhone, PendingAvatar,
        ///      PendingWeight, PendingExperienceYear) – KHÔNG ghi đè thông tin chính thức.
        ///   4. Đặt UpdateStatus = "Pending" để Admin biết có đơn cần xét duyệt.
        ///   5. Lưu vào DB và trả về true.
        ///
        /// === LÝ DO THIẾT KẾ ===
        ///   Dữ liệu chính thức (Phone, Avatar, Weight, ExperienceYear) chỉ được cập nhật
        ///   sau khi Admin chấp thuận → đảm bảo tính xác thực và kiểm soát của hệ thống.
        /// </summary>
        public async Task<bool> RequestUpdateProfileAsync(int jockeyId, UpdateJockeyDto dto)
        {
            // Bước 1: Tìm hồ sơ Jockey theo ID
            var profile = await _context.JockeyProfiles
                .FirstOrDefaultAsync(j => j.UserId == jockeyId);

            // Bước 2: Không tìm thấy → trả về false để controller trả 404
            if (profile == null) return false;

            // Bước 3: Lưu dữ liệu mới vào các cột Pending (không ghi đè chính thức)
            // Chỉ ghi nếu client gửi giá trị (null = không muốn thay đổi trường đó)
            if (dto.Phone != null)         profile.PendingPhone = dto.Phone;
            if (dto.Avatar != null)        profile.PendingAvatar = dto.Avatar;
            if (dto.Weight.HasValue)       profile.PendingWeight = dto.Weight;
            if (dto.ExperienceYear.HasValue) profile.PendingExperienceYear = dto.ExperienceYear;

            // Bước 4: Đánh dấu trạng thái "Pending" để Admin biết có đơn chờ duyệt
            profile.UpdateStatus = "Pending";
            profile.UpdateRequestedAt = DateTime.Now;

            // Bước 5: Lưu thay đổi vào database
            await _context.SaveChangesAsync();
            return true;
        }

        // =====================================================================
        // FR-JCKY-004 (Admin): Admin duyệt hoặc từ chối đơn cập nhật của Jockey
        // Endpoint: PUT /api/jockeys/{id}/review
        // =====================================================================

        /// <summary>
        /// Admin xem xét đơn cập nhật thông tin của Jockey và quyết định Approve / Reject.
        ///
        /// === LUỒNG HOẠT ĐỘNG ===
        ///   1. Tìm JockeyProfile có UpdateStatus = "Pending" theo jockeyId.
        ///   2. Nếu không có đơn pending hoặc không tìm thấy jockey → trả về false.
        ///   3a. Nếu Admin DUYỆT (IsApproved = true):
        ///       → Copy dữ liệu từ Pending sang cột chính thức (Phone, Avatar, Weight, ExperienceYear).
        ///       → Xóa các cột Pending (đặt về null).
        ///       → Đặt UpdateStatus = "Approved".
        ///   3b. Nếu Admin TỪ CHỐI (IsApproved = false):
        ///       → Chỉ xóa dữ liệu Pending (đặt về null), thông tin chính thức giữ nguyên.
        ///       → Đặt UpdateStatus = "Rejected".
        ///   4. Lưu AdminId và ghi chú vào trường audit (ReviewedBy, ReviewNotes).
        ///   5. Lưu vào DB và trả về true.
        ///
        /// === LÝ DO THIẾT KẾ ===
        ///   Mô hình 2 lớp (Pending → Approved) đảm bảo Admin luôn kiểm soát được
        ///   những thay đổi quan trọng (đặc biệt là Weight) trước khi chúng có hiệu lực.
        /// </summary>
        public async Task<bool> ReviewUpdateRequestAsync(int jockeyId, ReviewJockeyDto dto)
        {
            // Bước 1: Tìm hồ sơ Jockey đang có đơn chờ duyệt
            var profile = await _context.JockeyProfiles
                .FirstOrDefaultAsync(j => j.UserId == jockeyId && j.UpdateStatus == "Pending");

            // Bước 2: Không có đơn pending → trả về false để controller trả 404
            if (profile == null) return false;

            if (dto.IsApproved)
            {
                // Bước 3a – DUYỆT: áp dụng dữ liệu Pending vào thông tin chính thức
                if (profile.PendingPhone != null)
                    profile.Phone = profile.PendingPhone;

                if (profile.PendingAvatar != null)
                    profile.Avatar = profile.PendingAvatar;

                if (profile.PendingWeight.HasValue)
                    profile.Weight = profile.PendingWeight;

                if (profile.PendingExperienceYear.HasValue)
                    profile.ExperienceYear = profile.PendingExperienceYear;

                profile.UpdateStatus = "Approved";
            }
            else
            {
                // Bước 3b – TỪ CHỐI: chỉ hủy dữ liệu Pending, giữ nguyên thông tin chính thức
                profile.UpdateStatus = "Rejected";
            }

            // Bước 4: Xóa dữ liệu pending sau khi xử lý (dù Approve hay Reject)
            profile.PendingPhone = null;
            profile.PendingAvatar = null;
            profile.PendingWeight = null;
            profile.PendingExperienceYear = null;

            // Lưu thông tin audit: ai duyệt, lúc nào, ghi chú gì
            profile.ReviewedBy = dto.ReviewedBy;
            profile.ReviewNotes = dto.Notes;
            profile.ReviewedAt = DateTime.Now;

            // Bước 5: Lưu vào database
            await _context.SaveChangesAsync();
            return true;
        }

        // =====================================================================
        // FR-JCKY-004 (Public): Hiển thị danh sách Jockey công khai cho tất cả người dùng
        // Endpoint: GET /api/jockeys
        // =====================================================================

        /// <summary>
        /// Trả về danh sách công khai tất cả Jockey trong hệ thống, kèm thông tin User.
        ///
        /// === LUỒNG HOẠT ĐỘNG ===
        ///   1. Query bảng Jockey_Profiles, Include thêm bảng Users để lấy FullName và Email.
        ///   2. Sắp xếp theo UserId giảm dần (mới nhất lên đầu).
        ///   3. Trả về toàn bộ danh sách (không phân trang ở tầng service, phân trang ở FE nếu cần).
        ///
        /// === LÝ DO THIẾT KẾ ===
        ///   Endpoint này không yêu cầu xác thực (AllowAnonymous) để cho phép công chúng
        ///   xem thông tin jockey trước khi đăng ký tài khoản, phù hợp với mục tiêu marketing.
        /// </summary>
        public async Task<IEnumerable<JockeyProfile>> GetAllJockeysPublicAsync()
        {
            // Bước 1: Lấy danh sách Jockey kèm thông tin User (FullName, Email)
            return await _context.JockeyProfiles
                .Include(j => j.User)             // Join sang bảng Users để lấy FullName/Email
                .OrderByDescending(j => j.UserId) // Jockey mới nhất hiển thị trên cùng
                .ToListAsync();
        }
    }
}
