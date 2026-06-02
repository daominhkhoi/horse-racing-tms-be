using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.Entities
{
    // Đổi "Users" thành tên bảng thực tế trong SQL Server của bạn (VD: "tblUser" hoặc "Users")
    [Table("Users")]
    public class User
    {
        [Key] // Đánh dấu đây là Khóa chính (Primary Key)
        [Column("UserId")] // Map chính xác với tên cột trong SQL
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Cột tự động tăng
        public int UserId { get; set; }

        [Column("Email")]
        [Required] // Bắt buộc phải có (NOT NULL dưới SQL)
        [StringLength(255)] // Nên giới hạn độ dài giống cấu trúc dưới SQL để an toàn
        public string Email { get; set; } = string.Empty;

        [Column("PasswordHash")]
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("RoleId")]
        public int RoleId { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
    }
}