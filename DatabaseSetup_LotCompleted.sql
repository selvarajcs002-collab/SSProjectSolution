USE [SSManagement];
GO

CREATE OR ALTER PROCEDURE sp_MarkLotCompleted
    @CompanyId INT,
    @StyleNo NVARCHAR(100),
    @DesignName NVARCHAR(100),
    @Colour NVARCHAR(100),
    @PoNo NVARCHAR(100) = NULL,
    @IsDeliveryChallan BIT,
    @SelectedDcNos NVARCHAR(MAX) = NULL,
    @EntryType NVARCHAR(50),
    @ConsumedSizesJson NVARCHAR(MAX) = NULL,
    @ConsumedMetersJson NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Scenario A: Size-based completion
    IF @EntryType = 'size' AND @ConsumedSizesJson IS NOT NULL
    BEGIN
        SELECT 
            JSON_VALUE(value, '$.Size') as SizeName,
            CAST(JSON_VALUE(value, '$.ConsumedQty') as INT) as ConsumedQty
        INTO #TempConsumedSizes
        FROM OPENJSON(@ConsumedSizesJson);

        IF @IsDeliveryChallan = 1 AND @SelectedDcNos IS NOT NULL AND @SelectedDcNos <> ''
        BEGIN
            -- Update based on DC Number
            UPDATE ID
            SET ID.AvailableQty = CASE WHEN (ID.AvailableQty - TCS.ConsumedQty) < 0 THEN 0 ELSE (ID.AvailableQty - TCS.ConsumedQty) END,
                ID.Status = CASE WHEN (ID.AvailableQty - TCS.ConsumedQty) <= 0 THEN 'Completed' ELSE ID.Status END
            FROM InwardDetails ID
            INNER JOIN #TempConsumedSizes TCS ON UPPER(ID.SizeName) = UPPER(TCS.SizeName)
            WHERE ID.CompanyId = @CompanyId 
              AND ID.StyleNo = @StyleNo 
              AND ID.DesignName = @DesignName 
              AND ID.Colour = @Colour
              AND ID.DcNo IN (SELECT value FROM STRING_SPLIT(@SelectedDcNos, ','));
        END
        ELSE
        BEGIN
            -- Update based on PO/Style/Colour (No DC)
            UPDATE ISE
            SET ISE.AvailableQty = CASE WHEN (ISE.AvailableQty - TCS.ConsumedQty) < 0 THEN 0 ELSE (ISE.AvailableQty - TCS.ConsumedQty) END,
                ISE.Status = CASE WHEN (ISE.AvailableQty - TCS.ConsumedQty) <= 0 THEN 'Completed' ELSE ISE.Status END
            FROM InventoryStockEntry ISE
            INNER JOIN #TempConsumedSizes TCS ON UPPER(ISE.SizeName) = UPPER(TCS.SizeName)
            WHERE ISE.CompanyId = @CompanyId 
              AND ISE.StyleNo = @StyleNo 
              AND ISE.DesignName = @DesignName 
              AND ISE.Colour = @Colour
              AND (ISE.PoNo = @PoNo OR @PoNo = '' OR @PoNo IS NULL);
        END

        DROP TABLE #TempConsumedSizes;
    END

    -- Scenario B: Meter-based completion
    ELSE IF @EntryType = 'meter' AND @ConsumedMetersJson IS NOT NULL
    BEGIN
        SELECT 
            CAST(JSON_VALUE(value, '$.MeterPerBit') as DECIMAL(18,2)) as MeterPerBit,
            CAST(JSON_VALUE(value, '$.BitsCount') as INT) as BitsCount
        INTO #TempConsumedMeters
        FROM OPENJSON(@ConsumedMetersJson);

        IF @IsDeliveryChallan = 1 AND @SelectedDcNos IS NOT NULL AND @SelectedDcNos <> ''
        BEGIN
            UPDATE ID
            SET ID.AvailableBits = CASE WHEN (ID.AvailableBits - TCM.BitsCount) < 0 THEN 0 ELSE (ID.AvailableBits - TCM.BitsCount) END,
                ID.Status = CASE WHEN (ID.AvailableBits - TCM.BitsCount) <= 0 THEN 'Completed' ELSE ID.Status END
            FROM InwardDetails ID
            INNER JOIN #TempConsumedMeters TCM ON ID.MeterValue = TCM.MeterPerBit
            WHERE ID.CompanyId = @CompanyId 
              AND ID.StyleNo = @StyleNo 
              AND ID.DesignName = @DesignName 
              AND ID.Colour = @Colour
              AND ID.DcNo IN (SELECT value FROM STRING_SPLIT(@SelectedDcNos, ','));
        END
        ELSE
        BEGIN
            UPDATE ISE
            SET ISE.AvailableBits = CASE WHEN (ISE.AvailableBits - TCM.BitsCount) < 0 THEN 0 ELSE (ISE.AvailableBits - TCM.BitsCount) END,
                ISE.Status = CASE WHEN (ISE.AvailableBits - TCM.BitsCount) <= 0 THEN 'Completed' ELSE ISE.Status END
            FROM InventoryStockEntry ISE
            INNER JOIN #TempConsumedMeters TCM ON ISE.MeterValue = TCM.MeterPerBit
            WHERE ISE.CompanyId = @CompanyId 
              AND ISE.StyleNo = @StyleNo 
              AND ISE.DesignName = @DesignName 
              AND ISE.Colour = @Colour
              AND (ISE.PoNo = @PoNo OR @PoNo = '' OR @PoNo IS NULL);
        END

        DROP TABLE #TempConsumedMeters;
    END

    SELECT 1 AS success, 'Lot marked as completed successfully.' AS message;
END;
GO
