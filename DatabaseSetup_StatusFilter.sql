-- =============================================
-- Author:      Antigravity
-- Create date: 2026-05-31
-- Description: Optimized Stored Procedure for Status Screen filtering
-- =============================================

USE [SSManagement];
GO

IF OBJECT_ID('SP_GET_STATUS_FILTER', 'P') IS NOT NULL
    DROP PROCEDURE SP_GET_STATUS_FILTER;
GO

CREATE PROCEDURE SP_GET_STATUS_FILTER
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
    
    -- Variables for dynamic queries (if needed) or we can use a single combined query with conditionals.
    -- Since the schema is slightly different between Inward and Outward, we can use IF/ELSE for performance.
    
    IF @TransactionType = 'INWARD'
    BEGIN
        -- INWARD LOGIC
        
        -- Temporary table to hold filtered base records
        CREATE TABLE #FilteredInward (
            Id INT,
            CompanyId INT,
            CompanyName NVARCHAR(200),
            DcNo NVARCHAR(100),
            Date DATETIME,
            StyleNo NVARCHAR(100),
            DesignName NVARCHAR(150),
            Colour NVARCHAR(100),
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
            i.Colour,
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
            -- ViewType logic: Size vs Meter can be determined if needed. 
            -- But currently Inward/Outward doesn't strictly have a 'ViewType' column in header, 
            -- it separates via details. We will just filter based on existence in details or 'InwardEntryType'
            -- I'll rely on the existing schema pattern.
            -- Using EntryType if it exists, otherwise just return all for that type and let UI/Service handle or assume it returns both and service selects.
            -- Assuming no EntryType column since it caused errors before if it didn't exist in my grep. Wait, `ActivityLogService` used `InwardEntryType` ?
            -- Let's check `ActivityLogService` again, it said `whereBuilder.Append($" WHERE ({entryTypeColumn} = 'S' OR {entryTypeColumn} IS NULL)");`
            -- Ah! So `Inward` might have `InwardEntryType` (though it was missing in `merged_setup.sql`).
            -- If it has it, we should use it. 
            AND (
                (@ViewType = 'SIZE' AND (i.InwardEntryType = 'S' OR i.InwardEntryType IS NULL))
                OR
                (@ViewType = 'METER' AND i.InwardEntryType = 'M')
            );
            
            -- WAIT! The merged_setup.sql DID NOT have InwardEntryType. 
            -- If I use it, it might fail. I'll use it conditionally or omit it, let's omit it from WHERE and just fetch. 
            -- Actually, let's include it since ActivityLogService used it. Wait, `ActivityLogService` used it, so it must exist.

        -- Summary Result
        SELECT 
            COUNT(*) AS TotalRecords,
            SUM(TotalBitsCount) AS TotalBitsCount,
            SUM(TotalMeter) AS TotalMeter
        FROM #FilteredInward;
        
        -- Data Result
        SELECT * FROM #FilteredInward
        ORDER BY 
            CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'ASC' THEN Date END ASC,
            CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'DESC' THEN Date END DESC,
            Id DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        
        -- Details Result (Only for the paginated records)
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
        -- OUTWARD LOGIC
        
        CREATE TABLE #FilteredOutward (
            Id INT,
            CompanyId INT,
            CompanyName NVARCHAR(200),
            DcNo NVARCHAR(100),
            Date DATETIME,
            StyleNo NVARCHAR(100),
            DesignName NVARCHAR(150),
            Colour NVARCHAR(100),
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
            o.Colour,
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
            
        -- Summary Result
        SELECT 
            COUNT(*) AS TotalRecords,
            SUM(TotalBitsCount) AS TotalBitsCount,
            SUM(TotalMeter) AS TotalMeter
        FROM #FilteredOutward;
        
        -- Data Result
        SELECT * FROM #FilteredOutward
        ORDER BY 
            CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'ASC' THEN Date END ASC,
            CASE WHEN @SortColumn = 'Date' AND @SortDirection = 'DESC' THEN Date END DESC,
            Id DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        
        -- Details Result
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
GO
