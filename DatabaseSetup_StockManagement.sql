USE [SSManagementTEST]
GO

-- =============================================
-- Author:		Antigravity
-- Create date: 2026-07-01
-- Description:	Get Stock Summary for Stock Management
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetStockSummary]
(
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CompanyId INT = NULL,
    @StyleNo NVARCHAR(50) = NULL,
    @DesignName NVARCHAR(100) = NULL,
    @Colour NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalInwardQty INT = 0;
    DECLARE @TotalOutwardQty INT = 0;
    DECLARE @TodaysInwardQty INT = 0;
    DECLARE @TodaysOutwardQty INT = 0;

    -- Calculate Total Inward
    SELECT @TotalInwardQty = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND I.Status <> 'Deleted';

    -- Calculate Total Outward
    SELECT @TotalOutwardQty = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND O.Status <> 'Deleted';

    -- Calculate Todays Inward (ignoring FromDate/ToDate filter)
    SELECT @TodaysInwardQty = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND CAST(I.CreatedDate AS DATE) = CAST(GETDATE() AS DATE)
      AND I.Status <> 'Deleted';

    -- Calculate Todays Outward (ignoring FromDate/ToDate filter)
    SELECT @TodaysOutwardQty = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND CAST(O.CreatedDate AS DATE) = CAST(GETDATE() AS DATE)
      AND O.Status <> 'Deleted';

    SELECT 
        @TotalInwardQty AS TotalInwardQty,
        15.0 AS TotalInwardPercent, -- Dummy percent for UI
        @TotalOutwardQty AS TotalOutwardQty,
        10.0 AS TotalOutwardPercent, -- Dummy percent for UI
        (@TotalInwardQty - @TotalOutwardQty) AS AvailableStock,
        5.0 AS AvailableStockPercent, -- Dummy percent for UI
        @TodaysInwardQty AS TodaysInward,
        2.0 AS TodaysInwardPercent, -- Dummy percent for UI
        @TodaysOutwardQty AS TodaysOutward,
        -1.5 AS TodaysOutwardPercent, -- Dummy percent for UI
        15 AS LowStockItems -- Dummy count for UI
END
GO

-- =============================================
-- Author:		Antigravity
-- Create date: 2026-07-01
-- Description:	Get Stock Balance Size Wise
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetStockBalance_SizeWise]
(
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CompanyId INT = NULL,
    @StyleNo NVARCHAR(50) = NULL,
    @DesignName NVARCHAR(100) = NULL,
    @Colour NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    WITH InwardData AS (
        SELECT ISC.Size, SUM(ISC.[Count]) AS TotalInward
        FROM Inward I
        INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
        WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
          AND (@FromDate IS NULL OR CAST(I.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(I.CreatedDate AS DATE) <= @ToDate)
          AND I.Status <> 'Deleted'
        GROUP BY ISC.Size
    ),
    OutwardData AS (
        SELECT OSC.Size, SUM(OSC.[Count]) AS TotalOutward
        FROM Outward O
        INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
        WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
          AND (@FromDate IS NULL OR CAST(O.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(O.CreatedDate AS DATE) <= @ToDate)
          AND O.Status <> 'Deleted'
        GROUP BY OSC.Size
    )
    SELECT 
        COALESCE(I.Size, O.Size) AS Size,
        ISNULL(I.TotalInward, 0) AS TotalInward,
        ISNULL(O.TotalOutward, 0) AS TotalOutward,
        (ISNULL(I.TotalInward, 0) - ISNULL(O.TotalOutward, 0)) AS Available,
        (ISNULL(I.TotalInward, 0) - ISNULL(O.TotalOutward, 0)) AS Difference
    FROM InwardData I
    FULL OUTER JOIN OutwardData O ON I.Size = O.Size
    ORDER BY COALESCE(I.Size, O.Size);
END
GO

-- =============================================
-- Author:		Antigravity
-- Create date: 2026-07-01
-- Description:	Get Last Transactions
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetLastTransactions]
(
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CompanyId INT = NULL,
    @StyleNo NVARCHAR(50) = NULL,
    @DesignName NVARCHAR(100) = NULL,
    @Colour NVARCHAR(100) = NULL,
    @TopCount INT = 50
)
AS
BEGIN
    SET NOCOUNT ON;

    WITH AllTransactions AS (
        -- Inward
        SELECT 
            I.InwardId AS Id,
            I.CreatedDate AS [Date],
            'INWARD' AS [Type],
            I.InwardDcNo AS DcNo,
            C.CompanyName,
            I.StyleNo,
            I.DesignName,
            I.Colour AS Color,
            (SELECT ISNULL(SUM(ISC.[Count]), 0) FROM InwardSizeCount ISC WHERE ISC.InwardId = I.InwardId) AS InwardQty,
            NULL AS OutwardQty
        FROM Inward I
        LEFT JOIN CompanyDetails C ON I.CompanyId = C.CompanyId
        WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
          AND (@FromDate IS NULL OR CAST(I.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(I.CreatedDate AS DATE) <= @ToDate)
          AND I.Status <> 'Deleted'

        UNION ALL

        -- Outward
        SELECT 
            O.OutwardId AS Id,
            O.CreatedDate AS [Date],
            'OUTWARD' AS [Type],
            O.OutwardDcNo AS DcNo,
            C.CompanyName,
            O.StyleNo,
            O.DesignName,
            CASE 
                WHEN O.Colour = 'MULTI' THEN 
                    COALESCE(STUFF((SELECT ', ' + Colour FROM OutwardColour WHERE OutwardId = O.OutwardId FOR XML PATH('')), 1, 2, ''), O.Colour)
                ELSE O.Colour 
            END AS Color,
            NULL AS InwardQty,
            (SELECT ISNULL(SUM(OSC.[Count]), 0) FROM OutwardSizeCount OSC WHERE OSC.OutwardId = O.OutwardId) AS OutwardQty
        FROM Outward O
        LEFT JOIN CompanyDetails C ON O.CompanyId = C.CompanyId
        WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR O.Colour LIKE '%' + @Colour + '%' OR EXISTS (SELECT 1 FROM OutwardColour WHERE OutwardId = O.OutwardId AND Colour LIKE '%' + @Colour + '%'))
          AND (@FromDate IS NULL OR CAST(O.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(O.CreatedDate AS DATE) <= @ToDate)
          AND O.Status <> 'Deleted'
    )
    SELECT TOP (@TopCount) *
    FROM AllTransactions
    ORDER BY [Date] DESC;
END
GO
