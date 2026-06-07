USE [SSManagement];
GO

CREATE OR ALTER PROCEDURE sp_GetSizes_ByColour_Style
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @StyleNo NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    -- Inward Stock
    SELECT 
        isc.Size,
        SUM(isc.[Count]) AS TotalInward
    INTO #Inward
    FROM InwardSizeCount isc
    INNER JOIN Inward i ON isc.InwardId = i.InwardId
    WHERE i.CompanyId = @CompanyId
      AND i.Colour = @Colour
      AND i.StyleNo = @StyleNo
    GROUP BY isc.Size;

    -- Outward Used
    SELECT 
        osc.Size,
        SUM(osc.[Count]) AS TotalOutward
    INTO #Outward
    FROM OutwardSizeCount osc
    INNER JOIN Outward o ON osc.OutwardId = o.OutwardId
    WHERE o.CompanyId = @CompanyId
      AND o.Colour = @Colour
      AND o.StyleNo = @StyleNo
    GROUP BY osc.Size;

    SELECT 
        i.Size AS [size],
        (ISNULL(i.TotalInward, 0) - ISNULL(o.TotalOutward, 0)) AS [count],
        (ISNULL(i.TotalInward, 0) - ISNULL(o.TotalOutward, 0)) AS availableQty
    FROM #Inward i
    LEFT JOIN #Outward o ON i.Size = o.Size
    WHERE (ISNULL(i.TotalInward, 0) - ISNULL(o.TotalOutward, 0)) > 0;

    DROP TABLE #Inward;
    DROP TABLE #Outward;
END
GO
