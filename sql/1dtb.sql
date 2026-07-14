USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'HorseRacingDB')
BEGIN
    ALTER DATABASE HorseRacingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HorseRacingDB;
END
GO

CREATE DATABASE HorseRacingDB;
GO

USE HorseRacingDB;
GO
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255)
);
GO


INSERT INTO Roles (RoleName, Description)
VALUES 
    (N'Admin', N'System Administrator'),
    (N'HorseOwner', N'Horse Owner'),
    (N'Jockey', N'Jockey / Rider'),
    (N'Referee', N'Race Referee'),
    (N'Spectator', N'Spectator / Predictor');
GO

CREATE TABLE Users (
	FullName NVARCHAR(100) NOT NULL,
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    RoleID INT NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

CREATE TABLE Admin_Profiles (
    UserID INT PRIMARY KEY,
    Phone VARCHAR(20),
    Avatar VARCHAR(255),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);
GO

CREATE TABLE Owner_Profiles (
    UserID INT PRIMARY KEY,
    Phone VARCHAR(20),
    Avatar VARCHAR(255),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);
GO

CREATE TABLE Jockey_Profiles (
    UserID INT PRIMARY KEY,
    Phone VARCHAR(20),
    Avatar VARCHAR(255),
    ExperienceYear INT,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);
GO


CREATE TABLE Referee_Profiles (
    UserID INT PRIMARY KEY,
    Phone VARCHAR(20),
    Avatar VARCHAR(255),
    ExpYears INT,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);
GO

CREATE TABLE Spectator_Profiles (
    UserID INT PRIMARY KEY,
    Phone VARCHAR(20),
    Avatar VARCHAR(255),
    TotalPoints INT DEFAULT 0,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);
GO

CREATE TABLE Tournaments (
    TourID INT IDENTITY(1,1) PRIMARY KEY,
    TourName NVARCHAR(150) NOT NULL,
    StartDate DATE,
    EndDate DATE,
    Location NVARCHAR(255),
    PrizePool DECIMAL(18,2),
    Status NVARCHAR(50)
);
GO


CREATE TABLE Horses (
    HorseID INT IDENTITY(1,1) PRIMARY KEY,
    OwnerID INT NOT NULL,
    HorseName NVARCHAR(100) NOT NULL,
    Breed NVARCHAR(100),
    Age INT,
    Weight FLOAT,
    Gender NVARCHAR(20),
    HealthStatus NVARCHAR(50),
    FOREIGN KEY (OwnerID) REFERENCES Owner_Profiles(UserID)
);
GO


CREATE TABLE Horse_Verifications (
    VerifyID INT IDENTITY(1,1) PRIMARY KEY,
    HorseID INT NOT NULL,
    VerifiedBy INT, 
    VerifyDate DATETIME DEFAULT GETDATE(),
    InspectionURL VARCHAR(255),
    HealthCert_URL VARCHAR(255),
    Result NVARCHAR(50),
    Notes NVARCHAR(MAX),
    FOREIGN KEY (HorseID) REFERENCES Horses(HorseID),
    FOREIGN KEY (VerifiedBy) REFERENCES Referee_Profiles(UserID)
);
GO


CREATE TABLE Races (
    RaceID INT IDENTITY(1,1) PRIMARY KEY,
    TourID INT NOT NULL,
    RaceName NVARCHAR(150),
    Round INT,
    RaceDateTime DATETIME,
    Distance FLOAT,
    Status NVARCHAR(50),
    FOREIGN KEY (TourID) REFERENCES Tournaments(TourID)
);
GO


CREATE TABLE Referee_Assignments (
    AssignID INT IDENTITY(1,1) PRIMARY KEY,
    RaceID INT NOT NULL,
    RefereeID INT NOT NULL,
    AssignedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RaceID) REFERENCES Races(RaceID),
    FOREIGN KEY (RefereeID) REFERENCES Referee_Profiles(UserID)
);
GO


CREATE TABLE Race_Participants (
    ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
    RaceID INT NOT NULL,
    HorseID INT NOT NULL,
    JockeyID INT NOT NULL,
    LaneNumber INT,
    CheckInStatus NVARCHAR(50),
    ParticipationStatus NVARCHAR(50),
    CONSTRAINT UQ_Race_Horse_Jockey UNIQUE (RaceID, HorseID, JockeyID),
    FOREIGN KEY (RaceID) REFERENCES Races(RaceID),
    FOREIGN KEY (HorseID) REFERENCES Horses(HorseID),
    FOREIGN KEY (JockeyID) REFERENCES Jockey_Profiles(UserID)
);
GO


CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    RaceID INT NOT NULL,
    ParticipantID INT NOT NULL,
    FinishTime TIME,
    RankPosition INT,
    RewardMoney DECIMAL(18,2),
    ResultStatus NVARCHAR(50),
    FOREIGN KEY (RaceID) REFERENCES Races(RaceID),
    FOREIGN KEY (ParticipantID) REFERENCES Race_Participants(ParticipantID)
);
GO


CREATE TABLE Violations (
    ViolationID INT IDENTITY(1,1) PRIMARY KEY,
    RaceID INT NOT NULL,
    ParticipantID INT NOT NULL,
    RefereeID INT NOT NULL,
    ViolationType NVARCHAR(100),
    Penalty NVARCHAR(255),
    Description NVARCHAR(MAX),
    FOREIGN KEY (RaceID) REFERENCES Races(RaceID),
    FOREIGN KEY (ParticipantID) REFERENCES Race_Participants(ParticipantID),
    FOREIGN KEY (RefereeID) REFERENCES Referee_Profiles(UserID)
);
GO


CREATE TABLE Predictions (
    PredictionID INT IDENTITY(1,1) PRIMARY KEY,
    RaceID INT NOT NULL,
    SpectatorID INT NOT NULL,
    ParticipantID INT NOT NULL,
    BetPoints INT NOT NULL,
    Status NVARCHAR(50),
    RewardPoints INT,
    FOREIGN KEY (RaceID) REFERENCES Races(RaceID),
    FOREIGN KEY (SpectatorID) REFERENCES Spectator_Profiles(UserID),
    FOREIGN KEY (ParticipantID) REFERENCES Race_Participants(ParticipantID)
);
GO


CREATE TABLE Reward_Transactions (
    TranID INT IDENTITY(1,1) PRIMARY KEY,
    SpectatorID INT NOT NULL,
    PredictionID INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (SpectatorID) REFERENCES Spectator_Profiles(UserID),
    FOREIGN KEY (PredictionID) REFERENCES Predictions(PredictionID)
);
GO


CREATE TABLE Leaderboards (
    BoardID INT IDENTITY(1,1) PRIMARY KEY,
    TourID INT NOT NULL,
    HorseID INT NOT NULL,
    JockeyID INT NOT NULL,
    TotalPoints INT DEFAULT 0,
    TotalWins INT DEFAULT 0,
    Rank INT,
    FOREIGN KEY (TourID) REFERENCES Tournaments(TourID),
    FOREIGN KEY (HorseID) REFERENCES Horses(HorseID),
    FOREIGN KEY (JockeyID) REFERENCES Jockey_Profiles(UserID)
);
GO


CREATE TABLE Invitations (
    InviteID INT IDENTITY(1,1) PRIMARY KEY,
    OwnerID INT NOT NULL,
    JockeyID INT NOT NULL,
    HorseID INT NOT NULL,
    TourID INT NOT NULL,
    Message NVARCHAR(MAX),
    Status NVARCHAR(50),
    SentAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OwnerID) REFERENCES Owner_Profiles(UserID),
    FOREIGN KEY (JockeyID) REFERENCES Jockey_Profiles(UserID),
    FOREIGN KEY (HorseID) REFERENCES Horses(HorseID),
    FOREIGN KEY (TourID) REFERENCES Tournaments(TourID)
);
GO


CREATE TABLE RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsRevoked BIT NOT NULL DEFAULT 0, 
    RevokedAt DATETIME NULL, 
    
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

