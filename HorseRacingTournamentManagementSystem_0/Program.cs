using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Modules.Auth.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Auth.Services;
using HorseRacingTournamentManagementSystem_0.Modules.Horses.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Horses.Services;
using HorseRacingTournamentManagementSystem_0.Modules.Invitation.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Invitation.Services;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.Services;
using HorseRacingTournamentManagementSystem_0.Modules.Shared.Services;
using HorseRacingTournamentManagementSystem_0.Modules.Shared.Settings;
using HorseRacingTournamentManagementSystem_0.Modules.Users.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Users.Services;
using HorseRacingTournamentManagementSystem_0.Modules.Invitations.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Invitations.Services;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
// 1. THÊM 2 DÒNG USING NÀY CHO JWT
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

// Register EmailService
builder.Services.AddScoped<IEmailService, EmailService>();

// Register Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
// Register UserService
builder.Services.AddScoped<IUserService, UserService>();
// Register HorseService
builder.Services.AddScoped<IHorseService, HorseService>();
// Register TournamentService
builder.Services.AddScoped<HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Interfaces.ITournamentService, HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Services.TournamentService>();
// Register JockeyService
builder.Services.AddScoped<IJockeyService, JockeyService>();
// Register InvitationService
builder.Services.AddScoped<IInvitationService, InvitationService>();
// Register RaceService
builder.Services.AddScoped<IRaceService, RaceService>();

// --- Code cắm Database đã có sẵn của bạn ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HorseRacingDbContext>(options =>
    options.UseSqlServer(connectionString));

// CẦN CHỈNH SỬA THÊM: Đăng ký dịch vụ CORS cho React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            // Liệt kê rõ các origin – bắt buộc khi dùng AllowCredentials()
            // (Không được dùng AllowAnyOrigin() cùng với AllowCredentials())
            // Thêm nhiều port vì Vite tự động chọn port khác nếu port mặc định bị chiếm
            policy.WithOrigins(
                      "http://localhost:3000",  "https://localhost:3000",
                      "http://localhost:5173",  "https://localhost:5173",
                      "http://localhost:5174",  "https://localhost:5174",
                      "http://localhost:5175",  "https://localhost:5175",
                      "http://localhost:5176",  "https://localhost:5176",
                      "http://localhost:5177",  "https://localhost:5177",
                      "http://localhost:5178",  "https://localhost:5178"
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Cần thiết cho withCredentials: true ở FE (Login, Logout)
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
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name,
    };
})
.AddCookie("ExternalCookie")
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.SignInScheme = "ExternalCookie";
    options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HorseRacing API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CẦN CHỈNH SỬA THÊM: Kích hoạt CORS (Phải đứng trước Authentication)
app.UseCors("AllowReactApp");

// 3. THÊM DÒNG NÀY (BẮT BUỘC PHẢI NẰM TRƯỚC UseAuthorization)
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();