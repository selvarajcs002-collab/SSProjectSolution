-- =============================================
-- Script to create Meter-Based Inward tables and SPs
-- =============================================

USE [SSManagement];
GO

-- 1. Alter Table: Inward to add InwardEntryType
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Inward') AND name = 'InwardEntryType')
BEGIN
    ALTER TABLE Inward ADD InwardEntryType CHAR(1) NOT NULL DEFAULT 'S';
END
GO

-- 2. Create User Defined Table Type: MeterDetailType
IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'MeterDetailType')
BEGIN
    CREATE TYPE MeterDetailType AS TABLE (
        MeterValue DECIMAL(18,3) NOT NULL,
        BitsCount DECIMAL(18,3) NOT NULL
    );
END
GO

-- 3. Create Table: INWARD_METER_DETAIL
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'INWARD_METER_DETAIL')
BEGIN
    CREATE TABLE INWARD_METER_DETAIL
    (
        IMD_ID BIGINT IDENTITY(1,1) PRIMARY KEY,
        IMD_INWARD_ID INT NOT NULL,
        IMD_COMPANY_ID INT NULL,
        IMD_STYLE_ID INT NULL,
        IMD_DESIGN_ID INT NULL,
        IMD_METER_VALUE DECIMAL(18,3) NOT NULL,
        IMD_BITS_COUNT DECIMAL(18,3) NOT NULL,
        IMD_TOTAL_METER DECIMAL(18,3) NOT NULL,
        IMD_CREATED_BY INT NULL,
        IMD_CREATED_DATE DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_InwardMeterDetail_Inward FOREIGN KEY (IMD_INWARD_ID) REFERENCES Inward(InwardId) ON DELETE CASCADE
    )
END
GO

-- 4. Stored Procedure: SP_SAVE_INWARD_METER
IF OBJECT_ID('SP_SAVE_INWARD_METER', 'P') IS NOT NULL
    DROP PROCEDURE SP_SAVE_INWARD_METER;
GO

CREATE PROCEDURE SP_SAVE_INWARD_METER
    @InwardId INT,
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @DesignName NVARCHAR(150),
    @StyleNo NVARCHAR(100),
    @InwardDcNo NVARCHAR(100),
    @PoNo NVARCHAR(100) = NULL,
    @EntryType CHAR(1) = 'M',
    @CreatedBy INT,
    @MeterDetails MeterDetailType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Trim strings
        SET @Colour = LTRIM(RTRIM(@Colour));
        SET @DesignName = LTRIM(RTRIM(@DesignName));
        SET @StyleNo = LTRIM(RTRIM(@StyleNo));
        SET @InwardDcNo = ISNULL(LTRIM(RTRIM(@InwardDcNo)), '');
        IF @PoNo IS NOT NULL SET @PoNo = LTRIM(RTRIM(@PoNo));

        DECLARE @CurrentInwardId INT = @InwardId;

        -- Save inward master
        IF @CurrentInwardId = 0
        BEGIN
            INSERT INTO Inward (
                CompanyId, 
                Colour, 
                DesignName, 
                StyleNo, 
                InwardDcNo,
                PoNo,
                InwardEntryType,
                CreatedBy, 
                CreatedDate
            )
            VALUES (
                @CompanyId, 
                @Colour, 
                @DesignName, 
                @StyleNo, 
                @InwardDcNo,
                @PoNo,
                @EntryType,
                @CreatedBy, 
                GETDATE()
            );

            SET @CurrentInwardId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE Inward
            SET Colour = @Colour,
                DesignName = @DesignName,
                StyleNo = @StyleNo,
                InwardDcNo = @InwardDcNo,
                PoNo = @PoNo,
                UpdatedDate = GETDATE()
            WHERE InwardId = @CurrentInwardId AND CompanyId = @CompanyId;
        END

        -- Handle Meter Details
        -- Delete existing meter rows if it's an update (simple approach for edit, delete all and re-insert)
        -- Or we can do a MERGE, but Delete + Insert is straightforward.
        DELETE FROM INWARD_METER_DETAIL WHERE IMD_INWARD_ID = @CurrentInwardId;

        -- Insert new meter details
        -- Backend must recalculate Total Meter = Meter Value * Bits Count
        INSERT INTO INWARD_METER_DETAIL (
            IMD_INWARD_ID,
            IMD_COMPANY_ID,
            IMD_METER_VALUE,
            IMD_BITS_COUNT,
            IMD_TOTAL_METER,
            IMD_CREATED_BY,
            IMD_CREATED_DATE
        )
        SELECT 
            @CurrentInwardId,
            @CompanyId,
            MeterValue,
            BitsCount,
            (MeterValue * BitsCount), -- Backend calculation
            @CreatedBy,
            GETDATE()
        FROM @MeterDetails
        WHERE MeterValue > 0 AND BitsCount > 0; -- Validation

        COMMIT TRANSACTION;
        SELECT @CurrentInwardId AS InwardId, 'Meter Inward Saved Successfully' AS [Message];
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrorMessage, 16, 1);
    END CATCH
END
GO
