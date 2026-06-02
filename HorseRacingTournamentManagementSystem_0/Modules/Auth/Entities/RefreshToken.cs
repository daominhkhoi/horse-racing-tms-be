using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.Entities
{
    // Đừng quên sửa lại "RefreshTokens" cho khớp đúng tên bảng trong SQL của bạn nhé
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("UserId")]
        [Required]
        public int UserId { get; set; }

        // Khai báo khóa ngoại trỏ tới property UserId ở trên
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Column("Token")]
        [Required]
        // Nên set độ dài max nếu dưới SQL bạn dùng VARCHAR/NVARCHAR có giới hạn
        public string Token { get; set; } = string.Empty;

        [Column("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("IsRevoked")]
        public bool IsRevoked { get; set; }

        [Column("RevokedAt")]
        public DateTime? RevokedAt { get; set; } // Nullable vì token mới tạo chưa bị thu hồi
    }
}