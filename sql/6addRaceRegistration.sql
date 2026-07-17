USE HorseRacingDB;
GO

-- ============================================================
-- 1. Thêm các cột mới vào bảng Races
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Races' AND COLUMN_NAME = 'MinParticipants')
BEGIN
    ALTER TABLE Races ADD MinParticipants INT DEFAULT 4;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Races' AND COLUMN_NAME = 'MaxParticipants')
BEGIN
    ALTER TABLE Races ADD MaxParticipants INT DEFAULT 12;
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Races' AND COLUMN_NAME = 'CancelReason')
BEGIN
    ALTER TABLE Races ADD CancelReason NVARCHAR(500) NULL;
END
GO

-- ============================================================
-- 2. Tạo bảng Race_Registrations
--    (Horse Owner đăng ký Horse vào một Race cụ thể)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Race_Registrations')
BEGIN
    CREATE TABLE Race_Registrations (
        RegistrationID INT IDENTITY(1,1) PRIMARY KEY,
        RaceID         INT          NOT NULL,
        HorseID        INT          NOT NULL,
        OwnerID        INT          NOT NULL,
        Status         NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        RegisteredAt   DATETIME     NOT NULL DEFAULT GETDATE(),
        ReviewedAt     DATETIME     NULL,
        ReviewNote     NVARCHAR(500) NULL,
        CONSTRAINT UQ_RaceReg_Race_Horse UNIQUE (RaceID, HorseID),
        CONSTRAINT FK_RaceReg_Race  FOREIGN KEY (RaceID)   REFERENCES Races(RaceID),
        CONSTRAINT FK_RaceReg_Horse FOREIGN KEY (HorseID)  REFERENCES Horses(HorseID),
        CONSTRAINT FK_RaceReg_Owner FOREIGN KEY (OwnerID)  REFERENCES Owner_Profiles(UserID)
    );
END
GO

-- ============================================================
-- 3. Cho phép Race_Participants.JockeyID = NULL
--    (Horse được Approve trước khi chọn Jockey)
-- ============================================================

-- 3a. Xóa constraint UNIQUE cũ
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Race_Horse_Jockey' AND object_id = OBJECT_ID('Race_Participants'))
BEGIN
    ALTER TABLE Race_Participants DROP CONSTRAINT UQ_Race_Horse_Jockey;
END
GO

-- 3b. Đổi JockeyID thành nullable
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Race_Participants' AND COLUMN_NAME = 'JockeyID'
      AND IS_NULLABLE = 'NO'
)
BEGIN
    ALTER TABLE Race_Participants ALTER COLUMN JockeyID INT NULL;
END
GO

-- 3c. Tạo lại 2 unique indexes thay thế (cho phép nhiều NULL)
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Race_Horse_Nonnull_Jockey' AND object_id = OBJECT_ID('Race_Participants'))
BEGIN
    CREATE UNIQUE INDEX UQ_Race_Horse_Nonnull_Jockey
        ON Race_Participants (RaceID, HorseID)
        WHERE JockeyID IS NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Race_Horse_Jockey_Full' AND object_id = OBJECT_ID('Race_Participants'))
BEGIN
    CREATE UNIQUE INDEX UQ_Race_Horse_Jockey_Full
        ON Race_Participants (RaceID, HorseID, JockeyID)
        WHERE JockeyID IS NOT NULL;
END
GO

-- ============================================================
-- 4. Thêm RaceID vào bảng Invitations
--    (Để link một lời mời Jockey với Race cụ thể)
-- ============================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Invitations' AND COLUMN_NAME = 'RaceID')
BEGIN
    ALTER TABLE Invitations ADD RaceID INT NULL;
    ALTER TABLE Invitations ADD CONSTRAINT FK_Invitations_Races
        FOREIGN KEY (RaceID) REFERENCES Races(RaceID);
END
GO

-- ============================================================
-- 5. Cập nhật các Race hiện có sang status "Open Registration"
--    (Chỉ những Race đang "Upcoming" - tuỳ chọn)
-- ============================================================
-- UPDATE Races SET Status = 'Open Registration' WHERE Status = 'Upcoming';
-- GO

PRINT 'Migration 6addRaceRegistration completed successfully.';
GO
