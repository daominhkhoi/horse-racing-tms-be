USE HorseRacingDB;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Jockey_Profiles]') AND name = 'PendingPhone')
BEGIN
    ALTER TABLE [Jockey_Profiles] ADD 
        [PendingPhone] VARCHAR(20) NULL,
        [PendingAvatar] VARCHAR(255) NULL,
        [PendingExperienceYear] INT NULL,
        [UpdateStatus] VARCHAR(50) NULL,
        [UpdateRequestedAt] DATETIME NULL,
        [ReviewedBy] INT NULL,
        [ReviewNotes] NVARCHAR(MAX) NULL,
        [ReviewedAt] DATETIME NULL;
END
GO


------------------------------------------------------------------------------------------------------------------------------
--RUN CODE BELOW (30/06/2026)
------------------------------------------------------------------------------------------------------------------------------
-- Drop Weight columns if they still exist (migration cleanup)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Jockey_Profiles]') AND name = 'Weight')
BEGIN
    ALTER TABLE [Jockey_Profiles] DROP COLUMN [Weight];
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Jockey_Profiles]') AND name = 'PendingWeight')
BEGIN
    ALTER TABLE [Jockey_Profiles] DROP COLUMN [PendingWeight];
END
GO

-- Drop ExpYears column if it still exists (duplicated with ExperienceYear)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Jockey_Profiles]') AND name = 'ExpYears')
BEGIN
    ALTER TABLE [Jockey_Profiles] DROP COLUMN [ExpYears];
END
GO
