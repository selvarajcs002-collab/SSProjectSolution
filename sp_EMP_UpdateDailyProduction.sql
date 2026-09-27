CREATE OR ALTER PROCEDURE [dbo].[sp_EMP_UpdateDailyProduction]
    @Id INT = 0,
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

    IF EXISTS (SELECT 1 FROM EMP_DailyProductions WHERE Id = @Id)
    BEGIN
        UPDATE EMP_DailyProductions
        SET EmployeeName = @EmployeeName,
            MachineName = @MachineName,
            Shift = @Shift,
            StyleName = @StyleName,
            DesignName = @DesignName,
            TotalProduction = @TotalProduction,
            TargetProduction = @TargetProduction,
            CostPerPiece = @CostPerPiece,
            ProductionCost = @ProductionCost,
            Status = @Status,
            CompanyId = @CompanyId
        WHERE Id = @Id;

        SELECT @Id;
    END
    ELSE
    BEGIN
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
END
GO
