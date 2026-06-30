-- =============================================
-- Meter-Based Outward: Tables, UDTs, and Stored Procedure
-- Run this script on your SSManagement database.
-- Zero impact on existing Size-Based Outward tables or SPs.
-- =============================================

USE [SSManagement];
GO

-- =============================================
-- 1. ALTER TABLE Outward: Add OutwardEntryType column
--    'S' = Size Based (default, existing records unaffected)
--    'M' = Meter Based (new)
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('Outward') AND name = 'OutwardEntryType'
)
BEGIN
    ALTER TABLE Outward ADD OutwardEntryType CHAR(1) NOT NULL DEFAULT 'S';
    PRINT 'Column OutwardEntryType added to Outward table.';
END
ELSE
BEGIN
    PRINT 'Column OutwardEntryType already exists on Outward table.';
END
GO

-- =============================================
-- 2. CREATE User-Defined Table Type: OutwardMeterDetailType
--    Used as UDTT parameter in SP_SAVE_OUTWARD_METER
-- =============================================
-- NOTE: Must drop the stored procedure first because it depends on the UDTT
IF OBJECT_ID('SP_SAVE_OUTWARD_METER', 'P') IS NOT NULL
    DROP PROCEDURE SP_SAVE_OUTWARD_METER;
GO

IF EXISTS (SELECT * FROM sys.types WHERE name = 'OutwardMeterDetailType')
BEGIN
    DROP TYPE OutwardMeterDetailType;
    PRINT 'Dropped existing OutwardMeterDetailType.';
END
GO

CREATE TYPE OutwardMeterDetailType AS TABLE (
    MeterValue DECIMAL(18,3) NOT NULL,
    BitsCount  DECIMAL(18,3) NOT NULL
);
PRINT 'Type OutwardMeterDetailType created.';
GO

-- =============================================
-- 3. CREATE TABLE: OUTWARD_METER_DETAIL
--    Stores meter rows for each meter-based outward
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OUTWARD_METER_DETAIL')
BEGIN
    CREATE TABLE OUTWARD_METER_DETAIL
    (
        OMD_ID           BIGINT IDENTITY(1,1) PRIMARY KEY,
        OMD_OUTWARD_ID   BIGINT       NOT NULL,
        OMD_COMPANY_ID   BIGINT       NULL,
        OMD_STYLE_ID     BIGINT       NULL,
        OMD_DESIGN_ID    BIGINT       NULL,
        OMD_METER_VALUE  DECIMAL(18,3) NOT NULL,
        OMD_BITS_COUNT   DECIMAL(18,3) NOT NULL,
        OMD_TOTAL_METER  DECIMAL(18,3) NOT NULL,   -- Always backend-calculated
        OMD_CREATED_BY   BIGINT       NULL,
        OMD_CREATED_DATE DATETIME     DEFAULT GETDATE()
    );
    PRINT 'Table OUTWARD_METER_DETAIL created.';
END
ELSE
BEGIN
    PRINT 'Table OUTWARD_METER_DETAIL already exists.';
END
GO

-- =============================================
-- 4. STOCK VALIDATION HELPER VIEW (optional, for debugging)
--    Shows available meter stock per Company + Colour + MeterValue
-- =============================================
IF OBJECT_ID('vw_MeterStock_Available', 'V') IS NOT NULL
    DROP VIEW vw_MeterStock_Available;
GO

CREATE VIEW vw_MeterStock_Available
AS
    SELECT
        imd.IMD_COMPANY_ID                          AS CompanyId,
        imd.IMD_METER_VALUE                         AS MeterValue,
        ISNULL(SUM(imd.IMD_TOTAL_METER), 0)         AS TotalInward,
        ISNULL(omd.TotalOutward, 0)                 AS TotalOutward,
        ISNULL(SUM(imd.IMD_TOTAL_METER), 0)
            - ISNULL(omd.TotalOutward, 0)           AS AvailableMeter
    FROM INWARD_METER_DETAIL imd
    LEFT JOIN (
        SELECT
            OMD_COMPANY_ID,
            OMD_METER_VALUE,
            SUM(OMD_TOTAL_METER) AS TotalOutward
        FROM OUTWARD_METER_DETAIL
        GROUP BY OMD_COMPANY_ID, OMD_METER_VALUE
    ) omd
        ON imd.IMD_COMPANY_ID = omd.OMD_COMPANY_ID
       AND imd.IMD_METER_VALUE = omd.OMD_METER_VALUE
    GROUP BY
        imd.IMD_COMPANY_ID,
        imd.IMD_METER_VALUE,
        omd.TotalOutward;
GO

-- =============================================
-- 5. STORED PROCEDURE: SP_SAVE_OUTWARD_METER
--
-- Responsibilities:
--   - INSERT or UPDATE outward master record
--   - Auto-generate OutwardDcNo on INSERT
--   - Set OutwardEntryType = 'M'
--   - Per-row meter stock validation (METER-WISE, not total)
--   - Backend recalculates TotalMeter = MeterValue x BitsCount
--   - DELETE + re-INSERT detail rows (clean edit support)
--   - Full transaction with ROLLBACK on any error
--   - Returns Success, Message, OutwardId, OutwardDcNo
-- =============================================
IF OBJECT_ID('SP_SAVE_OUTWARD_METER', 'P') IS NOT NULL
    DROP PROCEDURE SP_SAVE_OUTWARD_METER;
GO

CREATE PROCEDURE SP_SAVE_OUTWARD_METER
    @OutwardId    INT,                         -- 0 = INSERT, >0 = UPDATE
    @CompanyId    INT,
    @StyleId      INT              = NULL,
    @DesignId     INT              = NULL,
    @Colour       NVARCHAR(100),
    @DesignName   NVARCHAR(150),
    @StyleNo      NVARCHAR(100),
    @EntryType    CHAR(1)          = 'M',
    @Mode         NVARCHAR(10)     = 'INSERT', -- 'INSERT' or 'UPDATE'
    @CreatedBy    NVARCHAR(100),
    @DeliveryTo   NVARCHAR(150)    = NULL,
    @PoNo         NVARCHAR(100)    = NULL,
    @Weight       NVARCHAR(100)    = NULL,
    @NoOfBundles  NVARCHAR(100)    = NULL,
    @UploadURL    NVARCHAR(500)    = NULL,
    @Status       NVARCHAR(50)     = NULL,
    @Remarks      NVARCHAR(MAX)    = NULL,
    @OutwardDate  DATETIME         = NULL,
    @MeterDetails OutwardMeterDetailType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ── Sanitise strings ──────────────────────────────────────────────────
        SET @Colour     = LTRIM(RTRIM(ISNULL(@Colour, '')));
        SET @DesignName = LTRIM(RTRIM(ISNULL(@DesignName, '')));
        SET @StyleNo    = LTRIM(RTRIM(ISNULL(@StyleNo, '')));
        SET @UploadURL  = LTRIM(RTRIM(ISNULL(@UploadURL, '')));
        SET @Status     = ISNULL(@Status, 'ACTIVE');
        SET @Remarks    = ISNULL(@Remarks, '');
        IF @OutwardDate IS NULL SET @OutwardDate = GETDATE();

        -- ── Guard: reject empty meter details ────────────────────────────────
        IF NOT EXISTS (SELECT 1 FROM @MeterDetails WHERE MeterValue > 0 AND BitsCount > 0)
        BEGIN
            SELECT
                CAST(0 AS BIT)          AS Success,
                'No valid meter details provided.' AS [Message],
                0                       AS OutwardId,
                ''                      AS OutwardDcNo;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        DECLARE @CurrentOutwardId INT    = @OutwardId;
        DECLARE @GeneratedDcNo    NVARCHAR(50) = '';

        -- ── INSERT or UPDATE outward master ───────────────────────────────────
        IF @CurrentOutwardId = 0 OR @Mode = 'INSERT'
        BEGIN
            -- Generate DC number: SSE_0012/2026-2027
            DECLARE @TotalCount INT;
            DECLARE @CurrentDate DATETIME = GETDATE();
            DECLARE @YearCurrent NVARCHAR(4);
            DECLARE @YearNext NVARCHAR(4);

            -- Financial year logic (April to March)
            IF MONTH(@CurrentDate) >= 4
            BEGIN
                SET @YearCurrent = CAST(YEAR(@CurrentDate) AS NVARCHAR(4));
                SET @YearNext = CAST(YEAR(@CurrentDate) + 1 AS NVARCHAR(4));
            END
            ELSE
            BEGIN
                SET @YearCurrent = CAST(YEAR(@CurrentDate) - 1 AS NVARCHAR(4));
                SET @YearNext = CAST(YEAR(@CurrentDate) AS NVARCHAR(4));
            END

            SELECT @TotalCount = ISNULL(COUNT(*), 0) + 1 FROM dbo.Outward;

            SET @GeneratedDcNo = CONCAT(
                'SSE_',
                RIGHT('0000' + CAST(@TotalCount AS NVARCHAR(10)), 4),
                '/',
                @YearCurrent,
                '-',
                @YearNext
            );

            INSERT INTO Outward (
                CompanyId,
                Colour,
                DesignName,
                StyleNo,
                OutwardDcNo,
                OutwardEntryType,
                CreatedBy,
                CreatedDate,
                Status,
                DeliveryTo,
                PoNo,
                Weight,
                NoOfBundles,
                UploadURL,
                Remarks,
                OutwardDate
            )
            VALUES (
                @CompanyId,
                @Colour,
                @DesignName,
                @StyleNo,
                @GeneratedDcNo,
                'M',
                @CreatedBy,
                GETDATE(),
                @Status,
                @DeliveryTo,
                @PoNo,
                @Weight,
                @NoOfBundles,
                @UploadURL,
                @Remarks,
                @OutwardDate
            );

            SET @CurrentOutwardId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- UPDATE: refresh header, keep existing DcNo
            SELECT @GeneratedDcNo = OutwardDcNo
            FROM   Outward
            WHERE  OutwardId = @CurrentOutwardId;

            UPDATE Outward
            SET    Colour          = @Colour,
                   DesignName      = @DesignName,
                   StyleNo         = @StyleNo,
                   OutwardEntryType = 'M',
                   DeliveryTo      = @DeliveryTo,
                   PoNo            = @PoNo,
                   Weight          = @Weight,
                   NoOfBundles     = @NoOfBundles,
                   UploadURL       = @UploadURL,
                   Status          = @Status,
                   Remarks         = @Remarks,
                   OutwardDate     = @OutwardDate,
                   UpdatedDate     = GETDATE()
            WHERE  OutwardId  = @CurrentOutwardId
              AND  CompanyId  = @CompanyId;
        END

        -- ── METER-WISE STOCK VALIDATION ───────────────────────────────────────
        -- For each requested meter row, independently verify available stock.
        -- Edit scenario: exclude THIS outward's previous usage from the stock check.
        -- ─────────────────────────────────────────────────────────────────────

        DECLARE @ValidationError NVARCHAR(500) = '';

        SELECT
            md.MeterValue,
            md.BitsCount,
            -- Backend recalculates: do NOT trust frontend TotalMeter
            (md.MeterValue * md.BitsCount) AS RequestedMeter,

            -- Total inward stock for this MeterValue + Company + Colour + Style
            ISNULL((
                SELECT SUM(imd.IMD_TOTAL_METER)
                FROM   INWARD_METER_DETAIL imd
                INNER JOIN Inward i ON imd.IMD_INWARD_ID = i.InwardId
                WHERE  imd.IMD_COMPANY_ID  = @CompanyId
                  AND  imd.IMD_METER_VALUE = md.MeterValue
                  AND  i.Colour            = @Colour
                  AND  i.StyleNo           = @StyleNo
            ), 0) AS InwardStock,

            -- Total outward EXCLUDING the current record (edit safety)
            ISNULL((
                SELECT SUM(omd.OMD_TOTAL_METER)
                FROM   OUTWARD_METER_DETAIL omd
                INNER JOIN Outward o ON omd.OMD_OUTWARD_ID = o.OutwardId
                WHERE  omd.OMD_COMPANY_ID  = @CompanyId
                  AND  omd.OMD_METER_VALUE = md.MeterValue
                  AND  o.Colour            = @Colour
                  AND  o.StyleNo           = @StyleNo
                  AND  omd.OMD_OUTWARD_ID <> @CurrentOutwardId  -- exclude current edit
            ), 0) AS OutwardUsed

        INTO #StockCheck
        FROM @MeterDetails md
        WHERE md.MeterValue > 0 AND md.BitsCount > 0;

        -- Check each row independently
        SELECT TOP 1
            @ValidationError = CONCAT(
                'Insufficient stock for MeterValue ',
                CAST(sc.MeterValue AS NVARCHAR(20)),
                '. Available: ',
                CAST((sc.InwardStock - sc.OutwardUsed) AS NVARCHAR(20)),
                ' MTR, Requested: ',
                CAST(sc.RequestedMeter AS NVARCHAR(20)),
                ' MTR.'
            )
        FROM #StockCheck sc
        WHERE sc.RequestedMeter > (sc.InwardStock - sc.OutwardUsed);

        IF @ValidationError <> ''
        BEGIN
            SELECT
                CAST(0 AS BIT)      AS Success,
                @ValidationError    AS [Message],
                0                   AS OutwardId,
                ''                  AS OutwardDcNo;

            DROP TABLE IF EXISTS #StockCheck;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        DROP TABLE IF EXISTS #StockCheck;

        -- ── Delete existing detail rows (clean slate for insert/update) ───────
        DELETE FROM OUTWARD_METER_DETAIL
        WHERE  OMD_OUTWARD_ID = @CurrentOutwardId;

        -- ── Insert fresh detail rows ──────────────────────────────────────────
        -- Backend recalculates TotalMeter = MeterValue × BitsCount.
        INSERT INTO OUTWARD_METER_DETAIL (
            OMD_OUTWARD_ID,
            OMD_COMPANY_ID,
            OMD_STYLE_ID,
            OMD_DESIGN_ID,
            OMD_METER_VALUE,
            OMD_BITS_COUNT,
            OMD_TOTAL_METER,    -- always backend-calculated
            OMD_CREATED_BY,
            OMD_CREATED_DATE
        )
        SELECT
            @CurrentOutwardId,
            @CompanyId,
            @StyleId,
            @DesignId,
            md.MeterValue,
            md.BitsCount,
            (md.MeterValue * md.BitsCount),   -- recalculate here, not from frontend
            TRY_CAST(@CreatedBy AS BIGINT),   -- Use TRY_CAST here to safely handle string date or numeric userId
            GETDATE()
        FROM @MeterDetails md
        WHERE md.MeterValue > 0 AND md.BitsCount > 0;

        COMMIT TRANSACTION;

        -- ── Return success ────────────────────────────────────────────────────
        SELECT
            CAST(1 AS BIT)                              AS Success,
            'Meter Outward Saved Successfully'          AS [Message],
            @CurrentOutwardId                           AS OutwardId,
            @GeneratedDcNo                              AS OutwardDcNo;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DROP TABLE IF EXISTS #StockCheck;

        DECLARE @ErrMsg  NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrLine INT            = ERROR_LINE();

        -- Return structured error instead of raising
        SELECT
            CAST(0 AS BIT)                                              AS Success,
            CONCAT('Error at line ', @ErrLine, ': ', @ErrMsg)          AS [Message],
            0                                                           AS OutwardId,
            ''                                                          AS OutwardDcNo;
    END CATCH
END
GO

PRINT 'SP_SAVE_OUTWARD_METER created successfully.';
GO
