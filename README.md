# 🏇 Horse Racing Tournament Management System API

Một hệ thống Web API quản lý giải đua ngựa mạnh mẽ, được xây dựng theo kiến trúc **Module-based** sạch sẽ và dễ mở rộng. Dự án hiện tại đang hoàn thiện phase quản lý tài khoản và bảo mật hệ thống.

## 🛠 Công nghệ sử dụng (Tech Stack)

* **Framework:** .NET (ASP.NET Core Web API)
* **Ngôn ngữ:** C#
* **Database:** SQL Server
* **ORM:** Entity Framework Core (Code-First)
* **Bảo mật & Mã hóa:** * JWT (JSON Web Token) cho Authentication & Authorization.
  * BCrypt.Net-Next để mã hóa mật khẩu (Password Hashing).

---

## ✨ Tính năng nổi bật (Features)

* [x] **Xác thực người dùng:** Đăng ký và Đăng nhập.
* [x] **Bảo mật tuyệt đối:** Mật khẩu không bao giờ lưu dạng plain-text nhờ thuật toán băm BCrypt.
* [x] **Xử lý Token:** Cấp phát JWT sống trong 2 giờ cho mỗi phiên đăng nhập hợp lệ.
* [x] **Phân quyền chặt chẽ (Role-based Access Control):** Hệ thống được thiết kế sẵn 5 vai trò biệt lập:
  1. `Admin` (Quản trị viên)
  2. `HorseOwner` (Chủ ngựa)
  3. `Jockey` (Kỵ sĩ)
  4. `Referee` (Trọng tài)
  5. `Spectator` (Khán giả)
* [x] Tích hợp sẵn giao diện thử nghiệm API qua **Swagger UI**.

---

## 📂 Cấu trúc thư mục cốt lõi

Dự án không dùng cấu trúc MVC mặc định mà chia theo **Modules** để dễ bảo trì khi dự án phình to:

```text
HorseRacingTournamentManagementSystem/
├── Modules/
│   └── Auth/
│       ├── Controllers/      # Nơi tiếp nhận API Request (AuthController)
│       ├── DTOs/             # Các khuôn chứa dữ liệu giao tiếp (Login, Register)
│       └── Entities/         # Định nghĩa cấu trúc Database (User, Role, DbContext)
├── appsettings.json          # Chứa Connection String và JWT Secret Key
└── Program.cs                # Nơi cấu hình Dependency Injection, JWT và Middleware
