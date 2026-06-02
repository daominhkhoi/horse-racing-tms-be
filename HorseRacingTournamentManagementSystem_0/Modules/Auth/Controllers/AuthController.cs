using HorseRacingTournamentManagementSystem_0.Modules.Auth.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Auth.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
// Thêm 3 using này để làm Token
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration; // Thêm biến đọc cấu hình

        // Bơm cả Database và Configuration vào
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            bool emailExists = _context.Users.Any(u => u.Email == request.Email);
            if (emailExists) return BadRequest(new { message = "Email is used!" });

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            int roleId = int.TryParse(request.Role, out int parsedRole) ? parsedRole : 5;

            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = hashedPassword,
                RoleId = roleId,
                IsActive = true
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { message = "Account registration successful!" });
        }

        // LOGIN VÀ TẠO JWT
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // 1. Tìm user theo Email
            var user = _context.Users.SingleOrDefault(u => u.Email == request.Email);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "The email address does not exist or is locked!" });
            }

            // 2. Kiểm tra Password (BƯỚC 10) - Hàm Verify của BCrypt tự động so sánh mk gốc và mk băm
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "The password is incorrect!" });
            }

            // 3. Lấy tên Role từ Database để nhét vào Token (VD: từ số 1 thành "Admin")
            var roleName = _context.Roles.SingleOrDefault(r => r.RoleId == user.RoleId)?.RoleName ?? "Spectator";

            // 4. BƯỚC 11 - TẠO JWT TOKEN
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName) // Phải có dòng này thì [Authorize(Roles="...")] mới hiểu
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(15), // Token sống được 2 tiếng
                signingCredentials: creds
            );

            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30), // Refresh Token sống 7 ngày
                IsRevoked = false // Đánh dấu token này vẫn đang hợp lệ
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            _context.SaveChanges();

            // 5. Trả Token về cho người dùng
            return Ok(new
            {
                message = "Login successful!",
                accessToken = accessTokenString,
                refreshToken = refreshTokenString
            });
        }

        [HttpPost("logout")]
        // Nếu bạn muốn bắt buộc user phải có Access Token hợp lệ mới được gọi API Logout, 
        // hãy bỏ comment dòng [Authorize] bên dưới:
        // [Authorize] 
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required!" });
            }

            // 1. Tìm Refresh Token trong database
            var tokenEntity = _context.RefreshTokens.SingleOrDefault(rt => rt.Token == request.RefreshToken);

            // 2. Nếu không tìm thấy, coi như người dùng gửi sai token, nhưng để bảo mật
            // và giúp Frontend dọn dẹp dễ dàng, ta vẫn trả về OK.
            if (tokenEntity == null)
            {
                return Ok(new { message = "Logout successful!" });
            }

            // 3. Nếu token đã bị thu hồi trước đó rồi (do người dùng click logout nhiều lần)
            if (tokenEntity.IsRevoked)
            {
                return Ok(new { message = "Logout successful!" });
            }

            // 4. Thực hiện "ám sát" (Revoke) token
            tokenEntity.IsRevoked = true;
            tokenEntity.RevokedAt = DateTime.UtcNow;

            // 5. Lưu xuống DB
            _context.SaveChanges();

            return Ok(new { message = "Logout successful!" });
        }
    }
}