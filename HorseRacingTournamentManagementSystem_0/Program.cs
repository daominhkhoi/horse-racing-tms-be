using HorseRacingTournamentManagementSystem_0.Modules.Auth.Entities;
// 1. THÊM 2 DÒNG USING NÀY CHO JWT
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Code cắm Database đã có sẵn của bạn ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
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
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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