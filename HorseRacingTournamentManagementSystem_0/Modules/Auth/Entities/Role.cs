using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.Entities
{
    // Tạm để tên bảng là "Roles", bạn nhớ kiểm tra lại cho khớp với SQL Server nhé
    [Table("Roles")]
    public class Role
    {
        [Key] // Đánh dấu Khóa chính
        [Column("RoleId")] // Map với cột RoleId
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Cột tự động tăng
        public int RoleId { get; set; }

        [Column("RoleName")]
        [Required] // Bắt buộc phải có (NOT NULL)
        [StringLength(50)] // Nên giới hạn độ dài (VD: NVARCHAR(50) trong SQL)
        public string RoleName { get; set; } = string.Empty;

        [Column("Description")]
        [StringLength(255)] // Tương tự, giới hạn độ dài cho mô tả
        public string Description { get; set; } = string.Empty;
    }
}