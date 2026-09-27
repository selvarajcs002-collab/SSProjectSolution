CREATE TABLE AdvanceAmount (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Date DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Remarks NVARCHAR(1000),
    CreatedBy NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
GO

CREATE PROCEDURE sp_GetAdvanceAmounts
AS
BEGIN
    SELECT Id, Name, Date, Amount, Remarks, CreatedBy, CreatedDate, IsActive 
    FROM AdvanceAmount 
    WHERE IsActive = 1
    ORDER BY Date DESC;
END
GO

CREATE PROCEDURE sp_GetAdvanceAmountById
    @Id INT
AS
BEGIN
    SELECT Id, Name, Date, Amount, Remarks, CreatedBy, CreatedDate, IsActive 
    FROM AdvanceAmount 
    WHERE Id = @Id AND IsActive = 1;
END
GO

CREATE PROCEDURE sp_InsertAdvanceAmount
    @Name NVARCHAR(255),
    @Date DATE,
    @Amount DECIMAL(18,2),
    @Remarks NVARCHAR(1000),
    @CreatedBy NVARCHAR(255)
AS
BEGIN
    INSERT INTO AdvanceAmount (Name, Date, Amount, Remarks, CreatedBy, CreatedDate, IsActive)
    VALUES (@Name, @Date, @Amount, @Remarks, @CreatedBy, GETDATE(), 1);
    
    SELECT SCOPE_IDENTITY();
END
GO

CREATE PROCEDURE sp_UpdateAdvanceAmount
    @Id INT,
    @Name NVARCHAR(255),
    @Date DATE,
    @Amount DECIMAL(18,2),
    @Remarks NVARCHAR(1000)
AS
BEGIN
    UPDATE AdvanceAmount
    SET Name = @Name,
        Date = @Date,
        Amount = @Amount,
        Remarks = @Remarks
    WHERE Id = @Id AND IsActive = 1;
END
GO

CREATE PROCEDURE sp_DeleteAdvanceAmount
    @Id INT
AS
BEGIN
    UPDATE AdvanceAmount
    SET IsActive = 0
    WHERE Id = @Id;
END
GO

CREATE PROCEDURE sp_GetAdvanceAmountsForExport
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SELECT Id, Name, Date, Amount, Remarks, CreatedBy, CreatedDate, IsActive 
    FROM AdvanceAmount 
    WHERE IsActive = 1 AND Date >= @FromDate AND Date <= @ToDate
    ORDER BY Date DESC;
END
GO
