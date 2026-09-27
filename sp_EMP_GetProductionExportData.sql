CREATE OR ALTER PROCEDURE [dbo].[sp_EMP_GetProductionExportData]
    @FromDate DATETIME,
    @ToDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    -- Ensure ToDate is inclusive of the entire day if it comes as a date without time
    -- If it comes with time like 23:59:59 it's fine, but just in case we add 1 day and use <
    DECLARE @AdjustedToDate DATETIME = DATEADD(day, 1, CAST(CAST(@ToDate AS DATE) AS DATETIME));
    DECLARE @AdjustedFromDate DATETIME = CAST(CAST(@FromDate AS DATE) AS DATETIME);

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
        EMP_DailyProductions
    WHERE 
        CreatedDate >= @AdjustedFromDate
        AND CreatedDate < @AdjustedToDate
    ORDER BY 
        CreatedDate ASC, Id ASC;
END
GO
