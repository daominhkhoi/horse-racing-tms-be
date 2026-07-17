CREATE TABLE TopupTransactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SpectatorId INT NOT NULL,
    Amount FLOAT NOT NULL,
    PointsAdded FLOAT NOT NULL,
    VnpTxnRef NVARCHAR(255) NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY (SpectatorId) REFERENCES Spectator_Profiles(UserID)
);
