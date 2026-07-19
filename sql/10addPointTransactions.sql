IF OBJECT_ID(N'[PointTransactions]', N'U') IS NULL
BEGIN
    CREATE TABLE [PointTransactions] (
        [TransactionId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SpectatorId] INT NOT NULL,
        [PredictionId] INT NULL,
        [Amount] FLOAT NOT NULL,
        [TransactionType] NVARCHAR(30) NOT NULL,
        [Description] NVARCHAR(250) NULL,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_PointTransactions_CreatedAt] DEFAULT GETDATE(),
        CONSTRAINT [FK_PointTransactions_Spectators] FOREIGN KEY ([SpectatorId]) REFERENCES [Spectator_Profiles]([UserID]),
        CONSTRAINT [FK_PointTransactions_Predictions] FOREIGN KEY ([PredictionId]) REFERENCES [Predictions]([PredictionID])
    );

    CREATE INDEX [IX_PointTransactions_SpectatorId_CreatedAt]
        ON [PointTransactions]([SpectatorId], [CreatedAt] DESC);

    CREATE UNIQUE INDEX [UX_PointTransactions_BetWon]
        ON [PointTransactions]([PredictionId])
        WHERE [TransactionType] = N'BetWon';

    CREATE UNIQUE INDEX [UX_PointTransactions_BetRefund]
        ON [PointTransactions]([PredictionId])
        WHERE [TransactionType] = N'BetRefund';
END;
GO
