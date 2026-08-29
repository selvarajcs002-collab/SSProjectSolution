CREATE OR ALTER PROCEDURE [dbo].[sp_EMP_GetAllDailyProductions]
    @Shift NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Replace 'EMP_DailyProduction' with your actual table name if it differs
    SELECT 
        Id,
        EmployeeName,
        MachineName,
        Shift,
        StyleName,
        DesignName,
        TotalProduction,
        TargetProduction,
        CostPerPiece,
        ProductionCost,
        Status,
        CompanyId,
        CreatedDate
    FROM 
        EMP_DailyProduction
    WHERE 
        (@Shift IS NULL OR Shift = @Shift)
    ORDER BY 
        Id DESC;
END
GO
