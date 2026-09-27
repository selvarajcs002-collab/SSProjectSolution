-- Alter Table
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ChallanNo' AND Object_ID = Object_ID(N'dbo.AdvanceAmount'))
BEGIN
    ALTER TABLE AdvanceAmount ADD ChallanNo NVARCHAR(50) NULL;
    ALTER TABLE AdvanceAmount ADD ChallanFileName NVARCHAR(255) NULL;
END
GO

-- Update SPs
CREATE OR ALTER PROCEDURE sp_GetAdvanceAmounts
AS
BEGIN
    SELECT Id, Name, Date, Amount, Remarks, CreatedBy, CreatedDate, IsActive, ChallanNo, ChallanFileName
    FROM AdvanceAmount 
    WHERE IsActive = 1
    ORDER BY Date DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_GetAdvanceAmountById
    @Id INT
AS
BEGIN
    SELECT Id, Name, Date, Amount, Remarks, CreatedBy, CreatedDate, IsActive, ChallanNo, ChallanFileName
    FROM AdvanceAmount 
    WHERE Id = @Id AND IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertAdvanceAmount
    @Name NVARCHAR(255),
    @Date DATE,
    @Amount DECIMAL(18,2),
    @Remarks NVARCHAR(1000),
    @CreatedBy NVARCHAR(255),
    @ChallanNo NVARCHAR(50) = NULL,
    @ChallanFileName NVARCHAR(255) = NULL
AS
BEGIN
    INSERT INTO AdvanceAmount (Name, Date, Amount, Remarks, CreatedBy, CreatedDate, IsActive, ChallanNo, ChallanFileName)
    VALUES (@Name, @Date, @Amount, @Remarks, @CreatedBy, GETDATE(), 1, @ChallanNo, @ChallanFileName);
    
    SELECT SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_UpdateAdvanceAmount
    @Id INT,
    @Name NVARCHAR(255),
    @Date DATE,
    @Amount DECIMAL(18,2),
    @Remarks NVARCHAR(1000),
    @ChallanNo NVARCHAR(50) = NULL,
    @ChallanFileName NVARCHAR(255) = NULL
AS
BEGIN
    UPDATE AdvanceAmount
    SET Name = @Name,
        Date = @Date,
        Amount = @Amount,
        Remarks = @Remarks,
        ChallanNo = COALESCE(@ChallanNo, ChallanNo),
        ChallanFileName = COALESCE(@ChallanFileName, ChallanFileName)
    WHERE Id = @Id AND IsActive = 1;
END
GO

-- New SP specifically for saving Challan details after generation
CREATE OR ALTER PROCEDURE sp_UpdateAdvanceAmountChallan
    @Id INT,
    @ChallanNo NVARCHAR(50),
    @ChallanFileName NVARCHAR(255)
AS
BEGIN
    UPDATE AdvanceAmount
    SET ChallanNo = @ChallanNo,
        ChallanFileName = @ChallanFileName
    WHERE Id = @Id;
END
GO
