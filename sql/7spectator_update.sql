-- Step 1: Add YoutubeId to Races table
ALTER TABLE [Races]
ADD [YoutubeId] NVARCHAR(50) NULL;
GO

-- Step 2: Create RaceComments table for Fan Reactions
CREATE TABLE [RaceComments] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [RaceId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [Content] NVARCHAR(500) NOT NULL,
    [CreatedAt] DATETIME DEFAULT GETDATE(),
    CONSTRAINT [FK_RaceComments_Races] FOREIGN KEY ([RaceId]) REFERENCES [Races]([RaceId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RaceComments_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([UserID]) ON DELETE CASCADE
);
GO
