
GO

IF OBJECT_ID('sp_GetDeliveryChallanExcelReport', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetDeliveryChallanExcelReport;
GO

CREATE PROCEDURE sp_GetDeliveryChallanExcelReport
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @Mode NVARCHAR(50), -- 'Inward' or 'Outward'
    @Type NVARCHAR(50), -- 'Size' or 'Meter'
    @CompanyId INT = NULL,
    @StyleNo NVARCHAR(100) = NULL,
    @DesignName NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Table to hold flat data
    CREATE TABLE #FlatData (
        Id INT,
        DCNo NVARCHAR(100),
        Date DATE,
        StyleNo NVARCHAR(100),
        DesignName NVARCHAR(150),
        Colour NVARCHAR(100),
        SizeName NVARCHAR(50),
        Quantity DECIMAL(18,2),
        MeterValue DECIMAL(18,2),
        TotalBits INT
    );

    IF @Mode = 'Inward' AND @Type = 'Size'
    BEGIN
        INSERT INTO #FlatData (Id, DCNo, Date, StyleNo, DesignName, Colour, SizeName, Quantity, MeterValue, TotalBits)
        SELECT 
            i.InwardId,
            i.InwardDcNo,
            CAST(i.CreatedDate AS DATE),
            i.StyleNo,
            i.DesignName,
            i.Colour,
            isc.Size,
            isc.Count,
            0, -- MeterValue
            isc.Count -- TotalBits
        FROM Inward i
        INNER JOIN InwardSizeCount isc ON i.InwardId = isc.InwardId
        WHERE (@FromDate IS NULL OR CAST(i.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(i.CreatedDate AS DATE) <= @ToDate)
          AND (@CompanyId IS NULL OR i.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR i.StyleNo = @StyleNo)
          AND (@DesignName IS NULL OR i.DesignName = @DesignName)
    END
    ELSE IF @Mode = 'Inward' AND @Type = 'Meter'
    BEGIN
        INSERT INTO #FlatData (Id, DCNo, Date, StyleNo, DesignName, Colour, SizeName, Quantity, MeterValue, TotalBits)
        SELECT 
            i.InwardId,
            i.InwardDcNo,
            CAST(i.CreatedDate AS DATE),
            i.StyleNo,
            i.DesignName,
            i.Colour,
            CAST(CAST(imd.IMD_METER_VALUE AS FLOAT) AS NVARCHAR(50)), -- Remove trailing zeros by casting to float first
            imd.IMD_TOTAL_METER, 
            imd.IMD_METER_VALUE,
            imd.IMD_BITS_COUNT
        FROM Inward i
        INNER JOIN INWARD_METER_DETAIL imd ON i.InwardId = imd.IMD_INWARD_ID
        WHERE (@FromDate IS NULL OR CAST(i.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(i.CreatedDate AS DATE) <= @ToDate)
          AND (@CompanyId IS NULL OR i.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR i.StyleNo = @StyleNo)
          AND (@DesignName IS NULL OR i.DesignName = @DesignName)
    END
    ELSE IF @Mode = 'Outward' AND @Type = 'Size'
    BEGIN
        INSERT INTO #FlatData (Id, DCNo, Date, StyleNo, DesignName, Colour, SizeName, Quantity, MeterValue, TotalBits)
        SELECT 
            o.OutwardId,
            o.OutwardDcNo,
            CAST(o.CreatedDate AS DATE),
            o.StyleNo,
            o.DesignName,
            o.Colour,
            osc.Size,
            osc.Count,
            0,
            osc.Count
        FROM Outward o
        INNER JOIN OutwardSizeCount osc ON o.OutwardId = osc.OutwardId
        WHERE (@FromDate IS NULL OR CAST(o.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(o.CreatedDate AS DATE) <= @ToDate)
          AND (@CompanyId IS NULL OR o.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR o.StyleNo = @StyleNo)
          AND (@DesignName IS NULL OR o.DesignName = @DesignName)
    END
    ELSE IF @Mode = 'Outward' AND @Type = 'Meter'
    BEGIN
        INSERT INTO #FlatData (Id, DCNo, Date, StyleNo, DesignName, Colour, SizeName, Quantity, MeterValue, TotalBits)
        SELECT 
            o.OutwardId,
            o.OutwardDcNo,
            CAST(o.CreatedDate AS DATE),
            o.StyleNo,
            o.DesignName,
            o.Colour,
            CAST(CAST(omd.OMD_METER_VALUE AS FLOAT) AS NVARCHAR(50)),
            omd.OMD_TOTAL_METER,
            omd.OMD_METER_VALUE,
            omd.OMD_BITS_COUNT
        FROM Outward o
        INNER JOIN OUTWARD_METER_DETAIL omd ON o.OutwardId = omd.OMD_OUTWARD_ID
        WHERE (@FromDate IS NULL OR CAST(o.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(o.CreatedDate AS DATE) <= @ToDate)
          AND (@CompanyId IS NULL OR o.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR o.StyleNo = @StyleNo)
          AND (@DesignName IS NULL OR o.DesignName = @DesignName)
    END

    -- Return 1: Flat Data
    SELECT DCNo, FORMAT(Date, 'dd-MM-yyyy') AS Date, StyleNo, DesignName, Colour, SizeName, Quantity, MeterValue, TotalBits
    FROM #FlatData
    ORDER BY Id DESC;

    -- Return 2: Summary
    SELECT 
        COUNT(DISTINCT Id) AS TotalRecords,
        ISNULL(SUM(TotalBits), 0) AS TotalBitsCount,
        ISNULL(SUM(CASE WHEN @Type = 'Meter' THEN Quantity ELSE 0 END), 0) AS TotalMeter
    FROM #FlatData;

    DROP TABLE #FlatData;
END
GO
