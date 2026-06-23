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

USE HorseRacingDB;
GO

CREATE TRIGGER trg_AfterUpdateUser_UpdateProfile
ON Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Chỉ xử lý khi RoleID thay đổi
    IF UPDATE(RoleID)
    BEGIN
        -------------------------------------------------
        -- XÓA PROFILE CŨ
        -------------------------------------------------

        -- Spectator cũ
        DELETE sp
        FROM Spectator_Profiles sp
        INNER JOIN deleted d ON sp.UserID = d.UserID
        INNER JOIN Roles r ON d.RoleID = r.RoleID
        WHERE r.RoleName = N'Spectator';

        -- Owner cũ
        DELETE op
        FROM Owner_Profiles op
        INNER JOIN deleted d ON op.UserID = d.UserID
        INNER JOIN Roles r ON d.RoleID = r.RoleID
        WHERE r.RoleName = N'HorseOwner';

        -- Jockey cũ
        DELETE jp
        FROM Jockey_Profiles jp
        INNER JOIN deleted d ON jp.UserID = d.UserID
        INNER JOIN Roles r ON d.RoleID = r.RoleID
        WHERE r.RoleName = N'Jockey';

        -- Referee cũ
        DELETE rp
        FROM Referee_Profiles rp
        INNER JOIN deleted d ON rp.UserID = d.UserID
        INNER JOIN Roles r ON d.RoleID = r.RoleID
        WHERE r.RoleName = N'Referee';

        -- Admin cũ
        DELETE ap
        FROM Admin_Profiles ap
        INNER JOIN deleted d ON ap.UserID = d.UserID
        INNER JOIN Roles r ON d.RoleID = r.RoleID
        WHERE r.RoleName = N'Admin';


        -------------------------------------------------
        -- THÊM PROFILE MỚI
        -------------------------------------------------

        -- Spectator mới
        INSERT INTO Spectator_Profiles (UserID)
        SELECT i.UserID
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID
        INNER JOIN Roles r ON i.RoleID = r.RoleID
        WHERE r.RoleName = N'Spectator'
              AND i.RoleID <> d.RoleID;

        -- Owner mới
        INSERT INTO Owner_Profiles (UserID)
        SELECT i.UserID
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID
        INNER JOIN Roles r ON i.RoleID = r.RoleID
        WHERE r.RoleName = N'HorseOwner'
              AND i.RoleID <> d.RoleID;

        -- Jockey mới
        INSERT INTO Jockey_Profiles (UserID)
        SELECT i.UserID
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID
        INNER JOIN Roles r ON i.RoleID = r.RoleID
        WHERE r.RoleName = N'Jockey'
              AND i.RoleID <> d.RoleID;

        -- Referee mới
        INSERT INTO Referee_Profiles (UserID)
        SELECT i.UserID
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID
        INNER JOIN Roles r ON i.RoleID = r.RoleID
        WHERE r.RoleName = N'Referee'
              AND i.RoleID <> d.RoleID;

        -- Admin mới
        INSERT INTO Admin_Profiles (UserID)
        SELECT i.UserID
        FROM inserted i
        INNER JOIN deleted d ON i.UserID = d.UserID
        INNER JOIN Roles r ON i.RoleID = r.RoleID
        WHERE r.RoleName = N'Admin'
              AND i.RoleID <> d.RoleID;
    END
END;
GO