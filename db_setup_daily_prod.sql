USE [SSManagementDEV];
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EMP_DailyProductions')
BEGIN
    CREATE TABLE EMP_DailyProductions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeName NVARCHAR(200) NULL,
        MachineName NVARCHAR(200) NULL,
        Shift NVARCHAR(50) NULL,
        StyleName NVARCHAR(200) NULL,
        DesignName NVARCHAR(200) NULL,
        TotalProduction INT NULL,
        TargetProduction INT NULL,
        CostPerPiece DECIMAL(18, 2) NULL,
        ProductionCost DECIMAL(18, 2) NULL,
        Status NVARCHAR(50) NULL,
        CompanyId INT NULL,
        CreatedDate DATETIME DEFAULT GETDATE()
    );
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_EMP_AddDailyProduction]
    @EmployeeName NVARCHAR(200) = NULL,
    @MachineName NVARCHAR(200) = NULL,
    @Shift NVARCHAR(50) = NULL,
    @StyleName NVARCHAR(200) = NULL,
    @DesignName NVARCHAR(200) = NULL,
    @TotalProduction INT = NULL,
    @TargetProduction INT = NULL,
    @CostPerPiece DECIMAL(18, 2) = NULL,
    @ProductionCost DECIMAL(18, 2) = NULL,
    @Status NVARCHAR(50) = NULL,
    @CompanyId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO EMP_DailyProductions (
        EmployeeName, MachineName, Shift, StyleName, DesignName,
        TotalProduction, TargetProduction, CostPerPiece, ProductionCost, Status, CompanyId
    )
    VALUES (
        @EmployeeName, @MachineName, @Shift, @StyleName, @DesignName,
        @TotalProduction, @TargetProduction, @CostPerPiece, @ProductionCost, @Status, @CompanyId
    );

    SELECT SCOPE_IDENTITY();
END
GO
