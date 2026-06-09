USE HorseRacingDB;
GO

CREATE TRIGGER trg_AfterInsertUser_CreateProfile
ON Users
AFTER INSERT
AS
BEGIN
    -- Ngăn chặn việc trả về số dòng bị ảnh hưởng, giúp tăng hiệu suất
    SET NOCOUNT ON;

    -- 1. Tạo Spectator Profile
    INSERT INTO Spectator_Profiles (UserID)
    SELECT i.UserID
    FROM inserted i
    INNER JOIN Roles r ON i.RoleID = r.RoleID
    WHERE r.RoleName = N'Spectator';

    -- 2. Tạo Owner Profile
    INSERT INTO Owner_Profiles (UserID)
    SELECT i.UserID
    FROM inserted i
    INNER JOIN Roles r ON i.RoleID = r.RoleID
    WHERE r.RoleName = N'HorseOwner';

    -- 3. Tạo Jockey Profile
    INSERT INTO Jockey_Profiles (UserID)
    SELECT i.UserID
    FROM inserted i
    INNER JOIN Roles r ON i.RoleID = r.RoleID
    WHERE r.RoleName = N'Jockey';

    -- 4. Tạo Referee Profile
    INSERT INTO Referee_Profiles (UserID)
    SELECT i.UserID
    FROM inserted i
    INNER JOIN Roles r ON i.RoleID = r.RoleID
    WHERE r.RoleName = N'Referee';

    -- 5. Tạo Admin Profile
    INSERT INTO Admin_Profiles (UserID)
    SELECT i.UserID
    FROM inserted i
    INNER JOIN Roles r ON i.RoleID = r.RoleID
    WHERE r.RoleName = N'Admin';
END;
GO