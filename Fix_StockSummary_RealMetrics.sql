USE [SSManagementTEST]
GO

-- Replaces the fixed 15 / 10 / 5 / 2 / -1.5 trend values and the fixed low-stock count of 15.
-- Trend compares the selected period with the previous period of the same length.
-- Today's cards compare with yesterday.
-- Needs-attention count matches the screen: sizes whose period net is 10 pcs or less.
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
    DECLARE @PrevInward INT = 0;
    DECLARE @PrevOutward INT = 0;
    DECLARE @YestInward INT = 0;
    DECLARE @YestOutward INT = 0;
    DECLARE @LowStockItems INT = 0;
    DECLARE @LowStockLimit INT = 10;
    DECLARE @PeriodDays INT = 30;
    DECLARE @PrevTo DATE;
    DECLARE @PrevFrom DATE;

    IF @FromDate IS NOT NULL AND @ToDate IS NOT NULL AND @ToDate >= @FromDate
        SET @PeriodDays = DATEDIFF(DAY, @FromDate, @ToDate) + 1;

    SET @PrevTo = DATEADD(DAY, -1, ISNULL(@FromDate, CAST(GETDATE() AS DATE)));
    SET @PrevFrom = DATEADD(DAY, -(@PeriodDays - 1), @PrevTo);

    SELECT @TotalInwardQty = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND (@FromDate IS NULL OR CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) >= @FromDate)
      AND (@ToDate IS NULL OR CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) <= @ToDate)
      AND I.Status <> 'Deleted';

    SELECT @TotalOutwardQty = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND (@FromDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) >= @FromDate)
      AND (@ToDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) <= @ToDate)
      AND O.Status <> 'Deleted';

    SELECT @TodaysInwardQty = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) = CAST(GETDATE() AS DATE)
      AND I.Status <> 'Deleted';

    SELECT @TodaysOutwardQty = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) = CAST(GETDATE() AS DATE)
      AND O.Status <> 'Deleted';

    SELECT @PrevInward = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) BETWEEN @PrevFrom AND @PrevTo
      AND I.Status <> 'Deleted';

    SELECT @PrevOutward = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) BETWEEN @PrevFrom AND @PrevTo
      AND O.Status <> 'Deleted';

    SELECT @YestInward = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) = DATEADD(DAY, -1, CAST(GETDATE() AS DATE))
      AND I.Status <> 'Deleted';

    SELECT @YestOutward = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) = DATEADD(DAY, -1, CAST(GETDATE() AS DATE))
      AND O.Status <> 'Deleted';

    ;WITH InwardData AS (
        SELECT ISC.Size, SUM(ISC.[Count]) AS TotalInward
        FROM Inward I
        INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
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
        WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
          AND (@FromDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) <= @ToDate)
          AND O.Status <> 'Deleted'
        GROUP BY OSC.Size
    )
    SELECT @LowStockItems = COUNT(*)
    FROM (
        SELECT (ISNULL(I.TotalInward, 0) - ISNULL(O.TotalOutward, 0)) AS Available
        FROM InwardData I
        FULL OUTER JOIN OutwardData O ON I.Size = O.Size
    ) Sizes
    WHERE Available <= @LowStockLimit;

    DECLARE @Available INT = @TotalInwardQty - @TotalOutwardQty;
    DECLARE @PrevAvailable INT = @PrevInward - @PrevOutward;

    SELECT
        @TotalInwardQty AS TotalInwardQty,
        CAST(CASE
            WHEN @PrevInward = 0 AND @TotalInwardQty = 0 THEN 0
            WHEN @PrevInward = 0 THEN 100
            ELSE ROUND(((@TotalInwardQty - @PrevInward) * 100.0) / @PrevInward, 1)
        END AS DECIMAL(10, 1)) AS TotalInwardPercent,
        @TotalOutwardQty AS TotalOutwardQty,
        CAST(CASE
            WHEN @PrevOutward = 0 AND @TotalOutwardQty = 0 THEN 0
            WHEN @PrevOutward = 0 THEN 100
            ELSE ROUND(((@TotalOutwardQty - @PrevOutward) * 100.0) / @PrevOutward, 1)
        END AS DECIMAL(10, 1)) AS TotalOutwardPercent,
        @Available AS AvailableStock,
        CAST(CASE
            WHEN @PrevAvailable = 0 AND @Available = 0 THEN 0
            WHEN @PrevAvailable = 0 THEN 100
            ELSE ROUND(((@Available - @PrevAvailable) * 100.0) / ABS(@PrevAvailable), 1)
        END AS DECIMAL(10, 1)) AS AvailableStockPercent,
        @TodaysInwardQty AS TodaysInward,
        CAST(CASE
            WHEN @YestInward = 0 AND @TodaysInwardQty = 0 THEN 0
            WHEN @YestInward = 0 THEN 100
            ELSE ROUND(((@TodaysInwardQty - @YestInward) * 100.0) / @YestInward, 1)
        END AS DECIMAL(10, 1)) AS TodaysInwardPercent,
        @TodaysOutwardQty AS TodaysOutward,
        CAST(CASE
            WHEN @YestOutward = 0 AND @TodaysOutwardQty = 0 THEN 0
            WHEN @YestOutward = 0 THEN 100
            ELSE ROUND(((@TodaysOutwardQty - @YestOutward) * 100.0) / @YestOutward, 1)
        END AS DECIMAL(10, 1)) AS TodaysOutwardPercent,
        @LowStockItems AS LowStockItems;
END
GO

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
    DECLARE @PrevInward INT = 0;
    DECLARE @PrevOutward INT = 0;
    DECLARE @YestInward INT = 0;
    DECLARE @YestOutward INT = 0;
    DECLARE @LowStockItems INT = 0;
    DECLARE @LowStockLimit INT = 10;
    DECLARE @PeriodDays INT = 30;
    DECLARE @PrevTo DATE;
    DECLARE @PrevFrom DATE;

    IF @FromDate IS NOT NULL AND @ToDate IS NOT NULL AND @ToDate >= @FromDate
        SET @PeriodDays = DATEDIFF(DAY, @FromDate, @ToDate) + 1;

    SET @PrevTo = DATEADD(DAY, -1, ISNULL(@FromDate, CAST(GETDATE() AS DATE)));
    SET @PrevFrom = DATEADD(DAY, -(@PeriodDays - 1), @PrevTo);

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

    SELECT @PrevInward = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    INNER JOIN @DcList dl ON dl.DcNo = I.InwardDcNo
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) BETWEEN @PrevFrom AND @PrevTo
      AND I.Status <> 'Deleted';

    SELECT @PrevOutward = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    INNER JOIN @DcList dl ON dl.DcNo = O.OutwardDcNo
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) BETWEEN @PrevFrom AND @PrevTo
      AND O.Status <> 'Deleted';

    SELECT @YestInward = ISNULL(SUM(ISC.[Count]), 0)
    FROM Inward I
    INNER JOIN InwardSizeCount ISC ON I.InwardId = ISC.InwardId
    INNER JOIN @DcList dl ON dl.DcNo = I.InwardDcNo
    WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(I.InwardDate, I.CreatedDate) AS DATE) = DATEADD(DAY, -1, CAST(GETDATE() AS DATE))
      AND I.Status <> 'Deleted';

    SELECT @YestOutward = ISNULL(SUM(OSC.[Count]), 0)
    FROM Outward O
    INNER JOIN OutwardSizeCount OSC ON O.OutwardId = OSC.OutwardId
    INNER JOIN @DcList dl ON dl.DcNo = O.OutwardDcNo
    WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
      AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
      AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
      AND (@Colour IS NULL OR OSC.Colour LIKE '%' + @Colour + '%')
      AND CAST(COALESCE(O.OutwardDate, O.CreatedDate) AS DATE) = DATEADD(DAY, -1, CAST(GETDATE() AS DATE))
      AND O.Status <> 'Deleted';

    ;WITH InwardData AS (
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
    SELECT @LowStockItems = COUNT(*)
    FROM (
        SELECT (ISNULL(I.TotalInward, 0) - ISNULL(O.TotalOutward, 0)) AS Available
        FROM InwardData I
        FULL OUTER JOIN OutwardData O ON I.Size = O.Size
    ) Sizes
    WHERE Available <= @LowStockLimit;

    DECLARE @Available INT = @TotalInwardQty - @TotalOutwardQty;
    DECLARE @PrevAvailable INT = @PrevInward - @PrevOutward;

    SELECT
        @TotalInwardQty AS TotalInwardQty,
        CAST(CASE
            WHEN @PrevInward = 0 AND @TotalInwardQty = 0 THEN 0
            WHEN @PrevInward = 0 THEN 100
            ELSE ROUND(((@TotalInwardQty - @PrevInward) * 100.0) / @PrevInward, 1)
        END AS DECIMAL(10, 1)) AS TotalInwardPercent,
        @TotalOutwardQty AS TotalOutwardQty,
        CAST(CASE
            WHEN @PrevOutward = 0 AND @TotalOutwardQty = 0 THEN 0
            WHEN @PrevOutward = 0 THEN 100
            ELSE ROUND(((@TotalOutwardQty - @PrevOutward) * 100.0) / @PrevOutward, 1)
        END AS DECIMAL(10, 1)) AS TotalOutwardPercent,
        @Available AS AvailableStock,
        CAST(CASE
            WHEN @PrevAvailable = 0 AND @Available = 0 THEN 0
            WHEN @PrevAvailable = 0 THEN 100
            ELSE ROUND(((@Available - @PrevAvailable) * 100.0) / ABS(@PrevAvailable), 1)
        END AS DECIMAL(10, 1)) AS AvailableStockPercent,
        @TodaysInwardQty AS TodaysInward,
        CAST(CASE
            WHEN @YestInward = 0 AND @TodaysInwardQty = 0 THEN 0
            WHEN @YestInward = 0 THEN 100
            ELSE ROUND(((@TodaysInwardQty - @YestInward) * 100.0) / @YestInward, 1)
        END AS DECIMAL(10, 1)) AS TodaysInwardPercent,
        @TodaysOutwardQty AS TodaysOutward,
        CAST(CASE
            WHEN @YestOutward = 0 AND @TodaysOutwardQty = 0 THEN 0
            WHEN @YestOutward = 0 THEN 100
            ELSE ROUND(((@TodaysOutwardQty - @YestOutward) * 100.0) / @YestOutward, 1)
        END AS DECIMAL(10, 1)) AS TodaysOutwardPercent,
        @LowStockItems AS LowStockItems;
END
GO
