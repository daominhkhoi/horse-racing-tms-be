using HorseRacingTournamentManagementSystem_0.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
// 1. THÊM 2 DÒNG USING NÀY CHO JWT
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HorseRacingTournamentManagementSystem_0.Modules.Auth.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Auth.Services;
using HorseRacingTournamentManagementSystem_0.Modules.Users.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Users.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

// Register EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

// Register UserService
builder.Services.AddScoped<IUserService, UserService>();

// --- Code cắm Database đã có sẵn của bạn ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HorseRacingDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==========================================
// CẦN CHỈNH SỬA THÊM: Đăng ký dịch vụ CORS cho React
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Cổng chạy React của bạn
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// 2. BƯỚC 12: CẤU HÌNH BẢO MẬT JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
})
.AddCookie("ExternalCookie")
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.SignInScheme = "ExternalCookie";
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==========================================
// CẦN CHỈNH SỬA THÊM: Kích hoạt CORS (Phải đứng trước Authentication)
// ==========================================
app.UseCors("AllowReactApp");

// 3. THÊM DÒNG NÀY (BẮT BUỘC PHẢI NẰM TRƯỚC UseAuthorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();