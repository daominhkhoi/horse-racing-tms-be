USE HorseRacingDB;
GO

DECLARE @ConstraintName nvarchar(200);

-- ==============================================================
-- 1. Xử lý bảng Spectator_Profiles (Cột TotalPoints)
-- ==============================================================
-- Xoá Default Constraint (nếu có)
SELECT @ConstraintName = Name FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('Spectator_Profiles') 
  AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('Spectator_Profiles'), 'TotalPoints', 'ColumnId');

IF @ConstraintName IS NOT NULL
    EXEC('ALTER TABLE Spectator_Profiles DROP CONSTRAINT ' + @ConstraintName);

-- Đổi sang FLOAT
ALTER TABLE Spectator_Profiles ALTER COLUMN TotalPoints FLOAT;

-- Thêm lại Default Constraint bằng 0
ALTER TABLE Spectator_Profiles ADD DEFAULT 0 FOR TotalPoints;
GO

-- ==============================================================
-- 2. Xử lý bảng Leaderboards (Cột TotalPoints)
-- ==============================================================
DECLARE @ConstraintName nvarchar(200);

-- Xoá Default Constraint (nếu có)
SELECT @ConstraintName = Name FROM sys.default_constraints 
WHERE parent_object_id = OBJECT_ID('Leaderboards') 
  AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('Leaderboards'), 'TotalPoints', 'ColumnId');

IF @ConstraintName IS NOT NULL
    EXEC('ALTER TABLE Leaderboards DROP CONSTRAINT ' + @ConstraintName);

-- Đổi sang FLOAT
ALTER TABLE Leaderboards ALTER COLUMN TotalPoints FLOAT;

-- Thêm lại Default Constraint bằng 0
ALTER TABLE Leaderboards ADD DEFAULT 0 FOR TotalPoints;
GO

-- ==============================================================
-- 3. Xử lý bảng Predictions (Cột BetPoints và RewardPoints)
-- ==============================================================
ALTER TABLE Predictions ALTER COLUMN BetPoints FLOAT NOT NULL;
ALTER TABLE Predictions ALTER COLUMN RewardPoints FLOAT;
GO

-- ==============================================================
-- 4. Thêm cột RewardRatio vào bảng Races (Kiểm tra trước khi thêm)
-- ==============================================================
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Races' AND COLUMN_NAME = 'RewardRatio'
)
BEGIN
    ALTER TABLE Races ADD RewardRatio FLOAT DEFAULT 2.0;
END
GO
