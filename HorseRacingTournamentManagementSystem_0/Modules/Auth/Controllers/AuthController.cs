using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Auth.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
// Thêm 3 using này để làm Token
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly HorseRacingDbContext _context;
        private readonly IConfiguration _configuration; // Thêm biến đọc cấu hình

        // Bơm cả Database và Configuration vào
        public AuthController(HorseRacingDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            request.Email = request.Email.Trim().ToLower();
            bool emailExists = _context.Users.Any(u => u.Email == request.Email);
            if (emailExists) return BadRequest(new { message = "Email is used!" });

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            int roleId = 5;

            var newUser = new User
            {
                FullName = request.FullName,
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
            request.Email = request.Email.Trim().ToLower();

            // 1. Tìm user theo Email
            var user = _context.Users.SingleOrDefault(u => u.Email == request.Email);
            if (user == null || user.IsActive != true)
            {
                return Unauthorized(new { message = "The email address does not exist or is locked!" });
            }

            // 2. Kiểm tra Password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "The password is incorrect!" });
            }

            // 3. Lấy tên Role từ Database
            var roleName = _context.Roles.SingleOrDefault(r => r.RoleId == user.RoleId)?.RoleName ?? "Spectator";

            // 4. TẠO JWT TOKEN
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(ClaimTypes.Name, user.FullName ?? "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(1), // Chỉnh lại theo comment của bạn (1 phút)
                signingCredentials: creds
            );

            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            // =========================================================================
            // CHỈNH SỬA TẠI ĐÂY: TÌM VÀ GHI ĐÈ REFRESH TOKEN NẾU ĐÃ CÓ, CHƯA CÓ THÌ TẠO MỚI
            // =========================================================================
            var existingRefreshToken = _context.RefreshTokens.SingleOrDefault(rt => rt.UserId == user.UserId);

            if (existingRefreshToken != null)
            {
                // Nếu đã có token trong DB -> Cập nhật (đè) token mới
                existingRefreshToken.Token = refreshTokenString;
                existingRefreshToken.CreatedAt = DateTime.Now;
                existingRefreshToken.ExpiresAt = DateTime.Now.AddDays(30);
                existingRefreshToken.IsRevoked = false;
                existingRefreshToken.RevokedAt = null; // Xóa thời gian thu hồi nếu trước đó token này từng bị logout
            }
            else
            {
                // Nếu chưa có -> Tạo mới hoàn toàn
                var newRefreshTokenEntity = new RefreshToken
                {
                    Token = refreshTokenString,
                    UserId = user.UserId,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddDays(30),
                    IsRevoked = false
                };
                _context.RefreshTokens.Add(newRefreshTokenEntity);
            }

            // Lưu thay đổi (EF Core sẽ tự hiểu là Update hoặc Insert dựa vào nhánh if/else ở trên)
            _context.SaveChanges();
            // =========================================================================

            // Set cookie cho Refresh Token
            Response.Cookies.Append(
                "refreshToken",
                refreshTokenString,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.Now.AddDays(30)
                });

            // 5. Trả Token về cho người dùng
            return Ok(new
            {
                message = "Login successful!",
                accessToken = accessTokenString
            });
        }

        [HttpPost("logout")]
        // Nếu bạn muốn bắt buộc user phải có Access Token hợp lệ mới được gọi API Logout, 
        // hãy bỏ comment dòng [Authorize] bên dưới:
        // [Authorize] 
        public IActionResult Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Ok(new { message = "Logout successful!" });
            }

            var tokenEntity = _context.RefreshTokens
                .SingleOrDefault(rt => rt.Token == refreshToken);

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
            tokenEntity.RevokedAt = DateTime.Now;

            // 5. Lưu xuống DB
            _context.SaveChanges();
            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Logout successful!" });
        }

        // ==========================================
        // TÍCH HỢP ĐĂNG NHẬP GOOGLE
        // ==========================================

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback))
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            // Đọc thông tin từ Cookie tạm do Google lưu sau khi login xong
            var result = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Lỗi xác thực Google." });
            }

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Không lấy được Email từ Google." });
            }

            // Kiểm tra user trong Database
            var user = _context.Users.SingleOrDefault(u => u.Email == email);
            if (user == null)
            {
                // Nếu chưa có, tạo tài khoản mới
                user = new User
                {
                    FullName = name ?? "Google User",
                    Email = email,
                    PasswordHash = "GoogleLoginNoPassword", // Không dùng tới
                    RoleId = 5, // RoleId 5 là mặc định (VD: Spectator)
                    IsActive = true
                };
                _context.Users.Add(user);
                _context.SaveChanges(); // Lưu để lấy được UserId
            }
            else if (user.IsActive != true)
            {
                return Unauthorized(new { message = "Tài khoản của bạn đã bị khóa!" });
            }

            // Lấy tên Role (giống y hệt lúc login thường)
            var roleName = _context.Roles.SingleOrDefault(r => r.RoleId == user.RoleId)?.RoleName ?? "Spectator";

            // TẠO JWT TOKEN CHO FRONTEND
            var tokenClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(ClaimTypes.Name, user.FullName ?? "Google User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: tokenClaims,
                expires: DateTime.Now.AddMinutes(1), // Hạn 1 phút như bạn đang cấu hình
                signingCredentials: creds
            );

            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            // Cập nhật RefreshToken
            var existingRefreshToken = _context.RefreshTokens.SingleOrDefault(rt => rt.UserId == user.UserId);
            if (existingRefreshToken != null)
            {
                existingRefreshToken.Token = refreshTokenString;
                existingRefreshToken.CreatedAt = DateTime.Now;
                existingRefreshToken.ExpiresAt = DateTime.Now.AddDays(30);
                existingRefreshToken.IsRevoked = false;
                existingRefreshToken.RevokedAt = null;
            }
            else
            {
                var newRefreshTokenEntity = new RefreshToken
                {
                    Token = refreshTokenString,
                    UserId = user.UserId,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddDays(30),
                    IsRevoked = false
                };
                _context.RefreshTokens.Add(newRefreshTokenEntity);
            }

            _context.SaveChanges();

            // Set cookie RefreshToken giống y hệt lúc Login bình thường
            Response.Cookies.Append(
                "refreshToken",
                refreshTokenString,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.Now.AddDays(30)
                });

            // Xóa cookie tạm thời của Google
            await HttpContext.SignOutAsync("ExternalCookie");

            // Chuyển hướng người dùng về trang Frontend React kèm theo Access Token
            // Lưu ý: Cổng React của bạn có thể là 5173, bạn đổi lại nếu cần nhé!
            var frontendUrl = $"http://localhost:5173/auth/callback?token={accessTokenString}";
            return Redirect(frontendUrl);
        }
    }
}
