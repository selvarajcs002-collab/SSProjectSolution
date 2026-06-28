ALTER PROCEDURE [dbo].[SP_GET_STATUS_FILTER]
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @CompanyId INT = NULL,
    @StyleId NVARCHAR(100) = NULL,
    @DesignId NVARCHAR(150) = NULL,
    @TransactionType NVARCHAR(20) = 'INWARD',
    @ViewType NVARCHAR(10) = 'SIZE',
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortColumn NVARCHAR(50) = 'Date',
    @SortDirection NVARCHAR(4) = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    IF @TransactionType = 'INWARD'
    BEGIN
        CREATE TABLE #FilteredInward (
            Id INT,
            CompanyId INT,
            CompanyName NVARCHAR(200),
            DcNo NVARCHAR(100),
            Date DATETIME,
            StyleNo NVARCHAR(100),
            DesignName NVARCHAR(150),
            Colour NVARCHAR(MAX),
            TotalBitsCount INT,
            TotalMeter DECIMAL(18,2)
        );

        INSERT INTO #FilteredInward (Id, CompanyId, CompanyName, DcNo, Date, StyleNo, DesignName, Colour, TotalBitsCount, TotalMeter)
        SELECT 
            i.InwardId,
            i.CompanyId,
            c.companyName,
            i.InwardDcNo,
            i.CreatedDate,
            i.StyleNo,
            i.DesignName,
            CASE 
                WHEN i.Colour = 'MULTI' THEN 
                    ISNULL(STUFF((SELECT DISTINCT ', ' + isc.Colour 
                           FROM InwardSizeCount isc 
                           WHERE isc.InwardId = i.InwardId 
                           FOR XML PATH('')), 1, 2, ''), 'MULTI')
                ELSE i.Colour 
            END AS Colour,
            CASE 
                WHEN i.InwardEntryType = 'M' THEN ISNULL((SELECT SUM(IMD_BITS_COUNT) FROM INWARD_METER_DETAIL imd WHERE imd.IMD_INWARD_ID = i.InwardId), 0)
                ELSE ISNULL((SELECT SUM(Count) FROM InwardSizeCount isc WHERE isc.InwardId = i.InwardId), 0)
            END AS TotalBitsCount,
            ISNULL((SELECT SUM(IMD_TOTAL_METER) FROM INWARD_METER_DETAIL imd WHERE imd.IMD_INWARD_ID = i.InwardId), 0) AS TotalMeter
        FROM Inward i
        LEFT JOIN CompanyDetails c ON i.CompanyId = c.companyId
        WHERE
            (@FromDate IS NULL OR CAST(i.CreatedDate AS DATE) >= @FromDate)
            AND (@ToDate IS NULL OR CAST(i.CreatedDate AS DATE) <= @ToDate)
            AND (@CompanyId IS NULL OR i.CompanyId = @CompanyId)
            AND (@StyleId IS NULL OR i.StyleNo = @StyleId)
            AND (@DesignId IS NULL OR i.DesignName = @DesignId)
            AND (
                (@ViewType = 'SIZE' AND (i.InwardEntryType = 'S' OR i.InwardEntryType IS NULL))
                OR
                (@ViewType = 'METER' AND i.InwardEntryType = 'M')
            );

        SELECT 
            COUNT(*) AS TotalRecords,
            SUM(TotalBitsCount) AS TotalBitsCount,
            SUM(TotalMeter) AS TotalMeter
        FROM #FilteredInward;
        
        SELECT * FROM #FilteredInward
        ORDER BY 
            CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'ASC' THEN Date END ASC,
            CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'DESC' THEN Date END DESC,
            Id DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        
        IF @ViewType = 'SIZE'
        BEGIN
            SELECT isc.Id, isc.InwardId AS ParentId, isc.Size, isc.Count 
            FROM InwardSizeCount isc
            INNER JOIN (
                SELECT Id FROM #FilteredInward
                ORDER BY 
                    CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'ASC' THEN Date END ASC,
                    CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'DESC' THEN Date END DESC,
                    Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            ) p ON isc.InwardId = p.Id;
        END
        ELSE
        BEGIN
            SELECT imd.IMD_ID AS Id, imd.IMD_INWARD_ID AS ParentId, imd.IMD_METER_VALUE AS MeterValue, imd.IMD_BITS_COUNT AS BitsCount, imd.IMD_TOTAL_METER AS TotalMeter
            FROM INWARD_METER_DETAIL imd
            INNER JOIN (
                SELECT Id FROM #FilteredInward
                ORDER BY 
                    CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'ASC' THEN Date END ASC,
                    CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'DESC' THEN Date END DESC,
                    Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            ) p ON imd.IMD_INWARD_ID = p.Id;
        END
        
        DROP TABLE #FilteredInward;
    END
    ELSE IF @TransactionType = 'OUTWARD'
    BEGIN
        CREATE TABLE #FilteredOutward (
            Id INT,
            CompanyId INT,
            CompanyName NVARCHAR(200),
            DcNo NVARCHAR(100),
            Date DATETIME,
            StyleNo NVARCHAR(100),
            DesignName NVARCHAR(150),
            Colour NVARCHAR(MAX),
            TotalBitsCount INT,
            TotalMeter DECIMAL(18,2)
        );

        INSERT INTO #FilteredOutward (Id, CompanyId, CompanyName, DcNo, Date, StyleNo, DesignName, Colour, TotalBitsCount, TotalMeter)
        SELECT 
            o.OutwardId,
            o.CompanyId,
            c.companyName,
            o.OutwardDcNo,
            o.CreatedDate,
            o.StyleNo,
            o.DesignName,
            CASE 
                WHEN o.Colour = 'MULTI' THEN 
                    ISNULL(STUFF((SELECT DISTINCT ', ' + osc.Colour 
                           FROM OutwardSizeCount osc 
                           WHERE osc.OutwardId = o.OutwardId 
                           FOR XML PATH('')), 1, 2, ''), 'MULTI')
                ELSE o.Colour 
            END AS Colour,
            CASE
                WHEN o.OutwardEntryType = 'M' THEN ISNULL((SELECT SUM(OMD_BITS_COUNT) FROM OUTWARD_METER_DETAIL omd WHERE omd.OMD_OUTWARD_ID = o.OutwardId), 0)
                ELSE ISNULL((SELECT SUM(Count) FROM OutwardSizeCount osc WHERE osc.OutwardId = o.OutwardId), 0)
            END AS TotalBitsCount,
            ISNULL((SELECT SUM(OMD_TOTAL_METER) FROM OUTWARD_METER_DETAIL omd WHERE omd.OMD_OUTWARD_ID = o.OutwardId), 0) AS TotalMeter
        FROM Outward o
        LEFT JOIN CompanyDetails c ON o.CompanyId = c.companyId
        WHERE
            (@FromDate IS NULL OR CAST(o.CreatedDate AS DATE) >= @FromDate)
            AND (@ToDate IS NULL OR CAST(o.CreatedDate AS DATE) <= @ToDate)
            AND (@CompanyId IS NULL OR o.CompanyId = @CompanyId)
            AND (@StyleId IS NULL OR o.StyleNo = @StyleId)
            AND (@DesignId IS NULL OR o.DesignName = @DesignId)
            AND (
                (@ViewType = 'SIZE' AND (o.OutwardEntryType = 'S' OR o.OutwardEntryType IS NULL))
                OR
                (@ViewType = 'METER' AND o.OutwardEntryType = 'M')
            );
            
        SELECT 
            COUNT(*) AS TotalRecords,
            SUM(TotalBitsCount) AS TotalBitsCount,
            SUM(TotalMeter) AS TotalMeter
        FROM #FilteredOutward;
        
        SELECT * FROM #FilteredOutward
        ORDER BY 
            CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'ASC' THEN Date END ASC,
            CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'DESC' THEN Date END DESC,
            Id DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        
        IF @ViewType = 'SIZE'
        BEGIN
            SELECT osc.Id, osc.OutwardId AS ParentId, osc.Size, osc.Count 
            FROM OutwardSizeCount osc
            INNER JOIN (
                SELECT Id FROM #FilteredOutward
                ORDER BY 
                    CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'ASC' THEN Date END ASC,
                    CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'DESC' THEN Date END DESC,
                    Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            ) p ON osc.OutwardId = p.Id;
        END
        ELSE
        BEGIN
            SELECT omd.OMD_ID AS Id, omd.OMD_OUTWARD_ID AS ParentId, omd.OMD_METER_VALUE AS MeterValue, omd.OMD_BITS_COUNT AS BitsCount, omd.OMD_TOTAL_METER AS TotalMeter
            FROM OUTWARD_METER_DETAIL omd
            INNER JOIN (
                SELECT Id FROM #FilteredOutward
                ORDER BY 
                    CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'ASC' THEN Date END ASC,
                    CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'DESC' THEN Date END DESC,
                    Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            ) p ON omd.OMD_OUTWARD_ID = p.Id;
        END
        
        DROP TABLE #FilteredOutward;
    END

END
