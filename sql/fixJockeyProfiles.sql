USE HorseRacingDB;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Jockey_Profiles]') AND name = 'PendingPhone')
BEGIN
    ALTER TABLE [Jockey_Profiles] ADD 
        [PendingPhone] VARCHAR(20) NULL,
        [PendingAvatar] VARCHAR(255) NULL,
        [PendingWeight] FLOAT NULL,
        [PendingExperienceYear] INT NULL,
        [UpdateStatus] VARCHAR(50) NULL,
        [UpdateRequestedAt] DATETIME NULL,
        [ReviewedBy] INT NULL,
        [ReviewNotes] NVARCHAR(MAX) NULL,
        [ReviewedAt] DATETIME NULL;
END
GO
