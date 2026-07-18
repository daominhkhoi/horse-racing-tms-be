/* Run after 8addTournamentBanner.sql. Safe to run more than once. */
IF COL_LENGTH('dbo.Races', 'MinParticipants') IS NULL
    ALTER TABLE dbo.Races ADD MinParticipants INT NOT NULL CONSTRAINT DF_Races_MinParticipants DEFAULT (2);
GO
IF COL_LENGTH('dbo.Races', 'MaxParticipants') IS NULL
    ALTER TABLE dbo.Races ADD MaxParticipants INT NOT NULL CONSTRAINT DF_Races_MaxParticipants DEFAULT (8);
GO
IF COL_LENGTH('dbo.Races', 'CancelReason') IS NULL
    ALTER TABLE dbo.Races ADD CancelReason NVARCHAR(500) NULL;
GO

IF OBJECT_ID('dbo.Race_Registrations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Race_Registrations (
        RegistrationID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RaceID INT NOT NULL,
        HorseID INT NOT NULL,
        OwnerID INT NOT NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_RaceRegistrations_Status DEFAULT ('Pending'),
        RegisteredAt DATETIME NOT NULL CONSTRAINT DF_RaceRegistrations_RegisteredAt DEFAULT (GETDATE()),
        ReviewedAt DATETIME NULL,
        ReviewedBy INT NULL,
        ReviewNote NVARCHAR(1000) NULL,
        CONSTRAINT UQ_RaceRegistrations_Race_Horse UNIQUE (RaceID, HorseID),
        CONSTRAINT FK_RaceRegistrations_Race FOREIGN KEY (RaceID) REFERENCES dbo.Races(RaceID),
        CONSTRAINT FK_RaceRegistrations_Horse FOREIGN KEY (HorseID) REFERENCES dbo.Horses(HorseID),
        CONSTRAINT FK_RaceRegistrations_Owner FOREIGN KEY (OwnerID) REFERENCES dbo.Owner_Profiles(UserID),
        CONSTRAINT FK_RaceRegistrations_Reviewer FOREIGN KEY (ReviewedBy) REFERENCES dbo.Users(UserID)
    );
END
GO

/* Upgrade an older Race_Registrations table without losing its data. */
IF COL_LENGTH('dbo.Race_Registrations', 'RegisteredAt') IS NULL
BEGIN
    ALTER TABLE dbo.Race_Registrations ADD RegisteredAt DATETIME NULL;
    IF COL_LENGTH('dbo.Race_Registrations', 'RegisterTime') IS NOT NULL
        EXEC('UPDATE dbo.Race_Registrations SET RegisteredAt = RegisterTime WHERE RegisteredAt IS NULL');
    UPDATE dbo.Race_Registrations SET RegisteredAt = GETDATE() WHERE RegisteredAt IS NULL;
    ALTER TABLE dbo.Race_Registrations ALTER COLUMN RegisteredAt DATETIME NOT NULL;
END
GO

IF COL_LENGTH('dbo.Race_Registrations', 'ReviewNote') IS NULL
BEGIN
    ALTER TABLE dbo.Race_Registrations ADD ReviewNote NVARCHAR(1000) NULL;
    IF COL_LENGTH('dbo.Race_Registrations', 'RejectReason') IS NOT NULL
        EXEC('UPDATE dbo.Race_Registrations SET ReviewNote = RejectReason WHERE ReviewNote IS NULL');
END
GO

IF COL_LENGTH('dbo.Race_Registrations', 'ReviewedBy') IS NULL
BEGIN
    ALTER TABLE dbo.Race_Registrations ADD ReviewedBy INT NULL;
    ALTER TABLE dbo.Race_Registrations ADD CONSTRAINT FK_RaceRegistrations_Reviewer
        FOREIGN KEY (ReviewedBy) REFERENCES dbo.Users(UserID);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RaceRegistrations_Race_Status' AND object_id = OBJECT_ID('dbo.Race_Registrations'))
    CREATE INDEX IX_RaceRegistrations_Race_Status ON dbo.Race_Registrations(RaceID, Status);
GO
