using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

/// <summary>
/// Entity ánh xạ tới bảng Jockey_Profiles trong database.
///
/// === MÔ HÌNH CẬP NHẬT 2 PHA (Two-phase update pattern) ===
///   Pha 1 – Jockey gửi đơn (FR-JCKY-002):
///       Dữ liệu mới được lưu vào các cột PendingXxx, UpdateStatus = "Pending".
///       Thông tin chính thức (Phone, Avatar, Weight, ExperienceYear) KHÔNG thay đổi.
///
///   Pha 2 – Admin xét duyệt (FR-JCKY-004):
///       • Approve → copy PendingXxx → chính thức, xóa Pending, UpdateStatus = "Approved".
///       • Reject  → xóa Pending, giữ nguyên chính thức,          UpdateStatus = "Rejected".
/// </summary>
public partial class JockeyProfile
{
    // ── THÔNG TIN CHÍNH THỨC (đã được Admin xác nhận) ──────────────────────
    public int UserId { get; set; }

    /// <summary>Số điện thoại chính thức, chỉ được cập nhật khi Admin duyệt.</summary>
    public string? Phone { get; set; }

    /// <summary>URL ảnh đại diện chính thức.</summary>
    public string? Avatar { get; set; }

    /// <summary>Số năm kinh nghiệm chính thức.</summary>
    public int? ExperienceYear { get; set; }

    // ── DỮ LIỆU PENDING (chờ Admin duyệt – FR-JCKY-002) ───────────────────
    /// <summary>Số điện thoại mới đang chờ Admin xét duyệt.</summary>
    public string? PendingPhone { get; set; }

    /// <summary>URL ảnh đại diện mới đang chờ Admin xét duyệt.</summary>
    public string? PendingAvatar { get; set; }


    /// <summary>Số năm kinh nghiệm mới đang chờ Admin xét duyệt.</summary>
    public int? PendingExperienceYear { get; set; }

    // ── TRẠNG THÁI & AUDIT ─────────────────────────────────────────────────
    /// <summary>
    /// Trạng thái đơn cập nhật: null | "Pending" | "Approved" | "Rejected".
    /// null = chưa có yêu cầu nào; "Pending" = đang chờ Admin duyệt.
    /// </summary>
    public string? UpdateStatus { get; set; }

    /// <summary>Thời điểm Jockey gửi đơn cập nhật.</summary>
    public DateTime? UpdateRequestedAt { get; set; }

    /// <summary>UserId của Admin đã xét duyệt đơn (audit trail).</summary>
    public int? ReviewedBy { get; set; }

    /// <summary>Ghi chú của Admin khi duyệt hoặc từ chối.</summary>
    public string? ReviewNotes { get; set; }

    /// <summary>Thời điểm Admin xét duyệt đơn.</summary>
    public DateTime? ReviewedAt { get; set; }

    // ── NAVIGATION PROPERTIES ───────────────────────────────────────────────
    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();

    public virtual ICollection<RaceParticipant> RaceParticipants { get; set; } = new List<RaceParticipant>();

    public virtual User User { get; set; } = null!;
}
