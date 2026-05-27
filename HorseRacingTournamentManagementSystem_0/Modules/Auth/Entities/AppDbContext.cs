using Microsoft.EntityFrameworkCore;
namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.Entities

{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Khai báo 2 bảng sẽ được tạo trong Database
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}