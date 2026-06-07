-- =============================================
-- Activity Log Module Stored Procedure
-- =============================================
USE [SSManagement];
GO

IF OBJECT_ID('SP_GET_ACTIVITY_LOG', 'P') IS NOT NULL
    DROP PROCEDURE SP_GET_ACTIVITY_LOG;
GO

CREATE PROCEDURE SP_GET_ACTIVITY_LOG
    @Module NVARCHAR(20),       -- 'INWARD' or 'OUTWARD'
    @ViewType CHAR(1),          -- 'S' or 'M'
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CompanyId BIGINT = NULL,
    @StyleNo NVARCHAR(100) = NULL,
    @DesignName NVARCHAR(150) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = 'Date',
    @SortDirection NVARCHAR(4) = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;

    -- Standardize variables
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Variables for Summary
    DECLARE @TotalRecords INT = 0;
    DECLARE @TotalBitsCount DECIMAL(18,3) = 0;
    DECLARE @TotalMeter DECIMAL(18,3) = 0;

    -- Temp table to hold filtered master data
    CREATE TABLE #FilteredData (
        Id BIGINT,
        CompanyId BIGINT,
        CompanyName NVARCHAR(200),
        DcNo NVARCHAR(100),
        Date DATETIME,
        StyleNo NVARCHAR(100),
        DesignName NVARCHAR(150),
        Colour NVARCHAR(100),
        TotalBitsCount DECIMAL(18,3),
        TotalMeter DECIMAL(18,3)
    );

    IF UPPER(@Module) = 'INWARD'
    BEGIN
        IF @ViewType = 'S'
        BEGIN
            INSERT INTO #FilteredData
            SELECT 
                i.InwardId,
                i.CompanyId,
                c.companyName,
                i.InwardDcNo,
                i.CreatedDate,
                i.StyleNo,
                i.DesignName,
                i.Colour,
                (SELECT SUM(isc.Count) FROM InwardSizeCount isc WHERE isc.InwardId = i.InwardId) AS TotalBitsCount,
                0 AS TotalMeter
            FROM Inward i
            LEFT JOIN CompanyDetails c ON i.CompanyId = c.companyId
            WHERE (i.InwardEntryType = 'S' OR i.InwardEntryType IS NULL)
              AND (@FromDate IS NULL OR CAST(i.CreatedDate AS DATE) >= @FromDate)
              AND (@ToDate IS NULL OR CAST(i.CreatedDate AS DATE) <= @ToDate)
              AND (@CompanyId IS NULL OR i.CompanyId = @CompanyId)
              AND (@StyleNo IS NULL OR @StyleNo = '' OR i.StyleNo = @StyleNo)
              AND (@DesignName IS NULL OR @DesignName = '' OR i.DesignName = @DesignName);
        END
        ELSE IF @ViewType = 'M'
        BEGIN
            INSERT INTO #FilteredData
            SELECT 
                i.InwardId,
                i.CompanyId,
                c.companyName,
                i.InwardDcNo,
                i.CreatedDate,
                i.StyleNo,
                i.DesignName,
                i.Colour,
                (SELECT SUM(imd.IMD_BITS_COUNT) FROM INWARD_METER_DETAIL imd WHERE imd.IMD_INWARD_ID = i.InwardId) AS TotalBitsCount,
                (SELECT SUM(imd.IMD_TOTAL_METER) FROM INWARD_METER_DETAIL imd WHERE imd.IMD_INWARD_ID = i.InwardId) AS TotalMeter
            FROM Inward i
            LEFT JOIN CompanyDetails c ON i.CompanyId = c.companyId
            WHERE i.InwardEntryType = 'M'
              AND (@FromDate IS NULL OR CAST(i.CreatedDate AS DATE) >= @FromDate)
              AND (@ToDate IS NULL OR CAST(i.CreatedDate AS DATE) <= @ToDate)
              AND (@CompanyId IS NULL OR i.CompanyId = @CompanyId)
              AND (@StyleNo IS NULL OR @StyleNo = '' OR i.StyleNo = @StyleNo)
              AND (@DesignName IS NULL OR @DesignName = '' OR i.DesignName = @DesignName);
        END
    END
    ELSE IF UPPER(@Module) = 'OUTWARD'
    BEGIN
        IF @ViewType = 'S'
        BEGIN
            INSERT INTO #FilteredData
            SELECT 
                o.OutwardId,
                o.CompanyId,
                c.companyName,
                o.OutwardDcNo,
                o.CreatedDate,
                o.StyleNo,
                o.DesignName,
                o.Colour,
                (SELECT SUM(osc.Count) FROM OutwardSizeCount osc WHERE osc.OutwardId = o.OutwardId) AS TotalBitsCount,
                0 AS TotalMeter
            FROM Outward o
            LEFT JOIN CompanyDetails c ON o.CompanyId = c.companyId
            WHERE (o.OutwardEntryType = 'S' OR o.OutwardEntryType IS NULL)
              AND (@FromDate IS NULL OR CAST(o.CreatedDate AS DATE) >= @FromDate)
              AND (@ToDate IS NULL OR CAST(o.CreatedDate AS DATE) <= @ToDate)
              AND (@CompanyId IS NULL OR o.CompanyId = @CompanyId)
              AND (@StyleNo IS NULL OR @StyleNo = '' OR o.StyleNo = @StyleNo)
              AND (@DesignName IS NULL OR @DesignName = '' OR o.DesignName = @DesignName);
        END
        ELSE IF @ViewType = 'M'
        BEGIN
            INSERT INTO #FilteredData
            SELECT 
                o.OutwardId,
                o.CompanyId,
                c.companyName,
                o.OutwardDcNo,
                o.CreatedDate,
                o.StyleNo,
                o.DesignName,
                o.Colour,
                (SELECT SUM(omd.OMD_BITS_COUNT) FROM OUTWARD_METER_DETAIL omd WHERE omd.OMD_OUTWARD_ID = o.OutwardId) AS TotalBitsCount,
                (SELECT SUM(omd.OMD_TOTAL_METER) FROM OUTWARD_METER_DETAIL omd WHERE omd.OMD_OUTWARD_ID = o.OutwardId) AS TotalMeter
            FROM Outward o
            LEFT JOIN CompanyDetails c ON o.CompanyId = c.companyId
            WHERE o.OutwardEntryType = 'M'
              AND (@FromDate IS NULL OR CAST(o.CreatedDate AS DATE) >= @FromDate)
              AND (@ToDate IS NULL OR CAST(o.CreatedDate AS DATE) <= @ToDate)
              AND (@CompanyId IS NULL OR o.CompanyId = @CompanyId)
              AND (@StyleNo IS NULL OR @StyleNo = '' OR o.StyleNo = @StyleNo)
              AND (@DesignName IS NULL OR @DesignName = '' OR o.DesignName = @DesignName);
        END
    END

    -- Get Totals
    SELECT @TotalRecords = COUNT(*), 
           @TotalBitsCount = ISNULL(SUM(TotalBitsCount), 0), 
           @TotalMeter = ISNULL(SUM(TotalMeter), 0)
    FROM #FilteredData;

    -- Return 1: Pagination and Summary metadata
    SELECT 
        @TotalRecords AS TotalRecords, 
        @TotalBitsCount AS TotalBitsCount, 
        @TotalMeter AS TotalMeter;

    -- Return 2: Paginated Data
    SELECT * FROM #FilteredData
    ORDER BY 
        CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'ASC' THEN [Date] END ASC,
        CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'DESC' THEN [Date] END DESC,
        CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'ASC' THEN DcNo END ASC,
        CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'DESC' THEN DcNo END DESC,
        CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'ASC' THEN StyleNo END ASC,
        CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'DESC' THEN StyleNo END DESC,
        Id DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Return 3: Detailed rows based on view type
    IF UPPER(@Module) = 'INWARD' AND @ViewType = 'S'
    BEGIN
        SELECT isc.Id, isc.InwardId AS ParentId, isc.Size, isc.Count 
        FROM InwardSizeCount isc
        INNER JOIN (
            SELECT Id FROM #FilteredData
            ORDER BY 
                CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'ASC' THEN [Date] END ASC,
                CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'DESC' THEN [Date] END DESC,
                CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'ASC' THEN DcNo END ASC,
                CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'DESC' THEN DcNo END DESC,
                CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'ASC' THEN StyleNo END ASC,
                CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'DESC' THEN StyleNo END DESC,
                Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        ) paginated ON isc.InwardId = paginated.Id;
    END
    ELSE IF UPPER(@Module) = 'INWARD' AND @ViewType = 'M'
    BEGIN
        SELECT imd.IMD_ID AS Id, imd.IMD_INWARD_ID AS ParentId, imd.IMD_METER_VALUE AS MeterValue, imd.IMD_BITS_COUNT AS BitsCount, imd.IMD_TOTAL_METER AS TotalMeter
        FROM INWARD_METER_DETAIL imd
        INNER JOIN (
            SELECT Id FROM #FilteredData
            ORDER BY 
                CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'ASC' THEN [Date] END ASC,
                CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'DESC' THEN [Date] END DESC,
                CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'ASC' THEN DcNo END ASC,
                CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'DESC' THEN DcNo END DESC,
                CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'ASC' THEN StyleNo END ASC,
                CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'DESC' THEN StyleNo END DESC,
                Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        ) paginated ON imd.IMD_INWARD_ID = paginated.Id;
    END
    ELSE IF UPPER(@Module) = 'OUTWARD' AND @ViewType = 'S'
    BEGIN
        SELECT osc.Id, osc.OutwardId AS ParentId, osc.Size, osc.Count 
        FROM OutwardSizeCount osc
        INNER JOIN (
            SELECT Id FROM #FilteredData
            ORDER BY 
                CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'ASC' THEN [Date] END ASC,
                CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'DESC' THEN [Date] END DESC,
                CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'ASC' THEN DcNo END ASC,
                CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'DESC' THEN DcNo END DESC,
                CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'ASC' THEN StyleNo END ASC,
                CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'DESC' THEN StyleNo END DESC,
                Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        ) paginated ON osc.OutwardId = paginated.Id;
    END
    ELSE IF UPPER(@Module) = 'OUTWARD' AND @ViewType = 'M'
    BEGIN
        SELECT omd.OMD_ID AS Id, omd.OMD_OUTWARD_ID AS ParentId, omd.OMD_METER_VALUE AS MeterValue, omd.OMD_BITS_COUNT AS BitsCount, omd.OMD_TOTAL_METER AS TotalMeter
        FROM OUTWARD_METER_DETAIL omd
        INNER JOIN (
            SELECT Id FROM #FilteredData
            ORDER BY 
                CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'ASC' THEN [Date] END ASC,
                CASE WHEN @SortColumn = 'Date' AND UPPER(@SortDirection) = 'DESC' THEN [Date] END DESC,
                CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'ASC' THEN DcNo END ASC,
                CASE WHEN @SortColumn = 'DcNo' AND UPPER(@SortDirection) = 'DESC' THEN DcNo END DESC,
                CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'ASC' THEN StyleNo END ASC,
                CASE WHEN @SortColumn = 'StyleNo' AND UPPER(@SortDirection) = 'DESC' THEN StyleNo END DESC,
                Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        ) paginated ON omd.OMD_OUTWARD_ID = paginated.Id;
    END

    DROP TABLE #FilteredData;
END
GO
