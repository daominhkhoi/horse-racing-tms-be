--CREATE TABLE HorseRacingDB
--USE HorseRacingDB

-- =============================================
-- 1. TẠO BẢNG ROLES (Bảng cha)
-- =============================================
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255) NULL
);
GO

-- Thêm sẵn data cho bảng Roles (dựa theo ảnh bạn chụp)
-- Thêm sẵn data cho bảng Roles (Mô tả tiếng Anh)
INSERT INTO Roles (RoleName, Description)
VALUES 
    (N'Admin', N'System Administrator'),
    (N'HorseOwner', N'Horse Owner'),
    (N'Jockey', N'Jockey / Rider'),
    (N'Referee', N'Race Referee'),
    (N'Spectator', N'Spectator / Predictor');
GO

-- =============================================
-- 2. TẠO BẢNG USERS (Bảng con của Roles, Bảng cha của RefreshTokens)
-- =============================================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE, -- Đảm bảo không có 2 user trùng email
    PasswordHash NVARCHAR(MAX) NOT NULL,
    RoleId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1, -- Mặc định tạo ra là Active (1)
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(), -- Tự động lấy giờ hiện tại
    
    -- Khai báo khoá ngoại trỏ tới bảng Roles
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
GO

-- =============================================
-- 3. TẠO BẢNG REFRESHTOKENS (Bảng con của Users)
-- =============================================
CREATE TABLE RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsRevoked BIT NOT NULL DEFAULT 0, -- Mặc định là chưa bị thu hồi (0)
    RevokedAt DATETIME NULL, -- Cho phép NULL vì token mới chưa bị thu hồi
    
    -- Khai báo khoá ngoại trỏ tới bảng Users
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO