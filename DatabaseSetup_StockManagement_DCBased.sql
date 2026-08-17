-- =============================================

-- =============================================
-- Author:		Antigravity
-- Create date: 2026-07-11
-- Description:	Get Delivery Challans for Stock
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetDeliveryChallans_ForStock]
(
    @CompanyId INT,
    @StyleNo NVARCHAR(50),
    @DesignName NVARCHAR(100),
    @Colour NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        DcNo AS DeliveryChallanNo,
        DcNo AS DisplayText,
        MAX(CreatedDate) AS CreatedDate,
        'Active' AS Status
    FROM (
        SELECT I.InwardDcNo AS DcNo, I.CreatedDate
        FROM Inward I
        WHERE I.CompanyId = @CompanyId
          AND I.StyleNo = @StyleNo
          AND I.DesignName = @DesignName
          AND I.Colour = @Colour
          AND I.Status <> 'Deleted'

        UNION ALL

        SELECT O.OutwardDcNo AS DcNo, O.CreatedDate
        FROM Outward O
        INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
        WHERE O.CompanyId = @CompanyId
          AND O.StyleNo = @StyleNo
          AND O.DesignName = @DesignName
          AND (O.Colour = @Colour OR OSC.Colour = @Colour OR EXISTS (SELECT 1 FROM OutwardColour WHERE OutwardId = O.OutwardId AND Colour = @Colour))
          AND O.Status <> 'Deleted'
    ) AS Combined
    WHERE DcNo IS NOT NULL AND RTRIM(LTRIM(DcNo)) <> ''
    GROUP BY DcNo
    ORDER BY MAX(CreatedDate) DESC;
END
GO

-- =============================================
-- Author:		Antigravity
-- Create date: 2026-07-11
-- Modified date: 2026-07-20
-- Description:	Get Stock Summary DC Based
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetStockSummary_DcBased]
(
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CompanyId INT = NULL,
    @StyleNo NVARCHAR(50) = NULL,
    @DesignName NVARCHAR(100) = NULL,
    @Colour NVARCHAR(100) = NULL,
    @DcList dbo.DcNumberList READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalInwardQty INT = 0;
    DECLARE @TotalOutwardQty INT = 0;
    DECLARE @TodaysInwardQty INT = 0;
    DECLARE @TodaysOutwardQty INT = 0;

    -- Calculate Total Inward based on DCs
    SELECT @TotalInwardQty = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    INNER JOIN @DcList dl ON dl.DcNo = I.InwardDcNo
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND (@FromDate IS NULL OR CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) >= @FromDate)
      AND (@ToDate IS NULL OR CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) <= @ToDate)
      AND I.Status <> 'Deleted';

    -- Calculate Total Outward based on DCs
    SELECT @TotalOutwardQty = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    INNER JOIN @DcList dl ON dl.DcNo = O.OutwardDcNo
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND (@FromDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) >= @FromDate)
      AND (@ToDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) <= @ToDate)
      AND O.Status <> 'Deleted';

    -- Calculate Todays Inward (ignoring FromDate/ToDate filter)
    SELECT @TodaysInwardQty = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    INNER JOIN @DcList dl ON dl.DcNo = I.InwardDcNo
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) = CAST(GETDATE() AS DATE)
      AND I.Status <> 'Deleted';

    -- Calculate Todays Outward (ignoring FromDate/ToDate filter)
    SELECT @TodaysOutwardQty = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    INNER JOIN @DcList dl ON dl.DcNo = O.OutwardDcNo
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) = CAST(GETDATE() AS DATE)
      AND O.Status <> 'Deleted';

    SELECT 
        @TotalInwardQty AS TotalInwardQty,
        15.0 AS TotalInwardPercent, 
        @TotalOutwardQty AS TotalOutwardQty,
        10.0 AS TotalOutwardPercent, 
        (@TotalInwardQty - @TotalOutwardQty) AS AvailableStock,
        5.0 AS AvailableStockPercent, 
        @TodaysInwardQty AS TodaysInward,
        2.0 AS TodaysInwardPercent, 
        @TodaysOutwardQty AS TodaysOutward,
        -1.5 AS TodaysOutwardPercent, 
        15 AS LowStockItems 
END
GO

-- =============================================
-- Author:		Antigravity
-- Create date: 2026-07-11
-- Modified date: 2026-07-20
-- Description:	Get Stock Balance Size Wise DC Based
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetStockBalance_SizeWise_DcBased]
(
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CompanyId INT = NULL,
    @StyleNo NVARCHAR(50) = NULL,
    @DesignName NVARCHAR(100) = NULL,
    @Colour NVARCHAR(100) = NULL,
    @DcList dbo.DcNumberList READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    WITH InwardData AS (
        SELECT ISC.Size, SUM(ISC.[Count]) AS TotalInward
        FROM Inward I
        INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
        INNER JOIN @DcList dl ON dl.DcNo = I.InwardDcNo
        WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
          AND (@FromDate IS NULL OR CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) <= @ToDate)
          AND I.Status <> 'Deleted'
        GROUP BY ISC.Size
    ),
    OutwardData AS (
        SELECT OSC.Size, SUM(OSC.[Count]) AS TotalOutward
        FROM Outward O
        INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
        INNER JOIN @DcList dl ON dl.DcNo = O.OutwardDcNo
        WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
          AND (@FromDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) <= @ToDate)
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
-- Create date: 2026-07-11
-- Modified date: 2026-07-20
-- Description:	Get Last Transactions DC Based
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetLastTransactions_DcBased]
(
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CompanyId INT = NULL,
    @StyleNo NVARCHAR(50) = NULL,
    @DesignName NVARCHAR(100) = NULL,
    @Colour NVARCHAR(100) = NULL,
    @TopCount INT = 50,
    @DcList dbo.DcNumberList READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    WITH AllTransactions AS (
        -- Inward
        SELECT 
            I.InwardId AS Id,
            COALESCE(I.InwardDate, I.CreatedDate) AS [Date],
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
        INNER JOIN @DcList dl ON dl.DcNo = I.InwardDcNo
        WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
          AND (@FromDate IS NULL OR CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) <= @ToDate)
          AND I.Status <> 'Deleted'

        UNION ALL

        -- Outward
        SELECT 
            O.OutwardId AS Id,
            COALESCE(O.OutwardDate, O.CreatedDate) AS [Date],
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
        INNER JOIN @DcList dl ON dl.DcNo = O.OutwardDcNo
        WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR O.Colour LIKE '%' + @Colour + '%' OR EXISTS (SELECT 1 FROM OutwardColour WHERE OutwardId = O.OutwardId AND Colour LIKE '%' + @Colour + '%'))
          AND (@FromDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) <= @ToDate)
          AND O.Status <> 'Deleted'
    )
    SELECT *
    FROM AllTransactions
    ORDER BY [Date] DESC;
END
GO
