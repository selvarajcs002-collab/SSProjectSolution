USE [SSManagementDEV]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Next outward DC number comes from DcNumberSequence, not from MAX of live Outward rows.
-- Deleted numbers stay recorded and are not issued again by this procedure.
CREATE OR ALTER PROCEDURE [dbo].[usp_GenerateOutwardDcNo]
(
    @OutwardDcNo   NVARCHAR(50) OUTPUT,
    @CreatedBy     NVARCHAR(100) = NULL,
    @CompanyId     INT = NULL,
    @AllocationId  BIGINT = NULL OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @YearCurrent NVARCHAR(4) = CAST(YEAR(GETDATE()) AS NVARCHAR(4));
    DECLARE @YearNext    NVARCHAR(4) = CAST(YEAR(GETDATE()) + 1 AS NVARCHAR(4));
    DECLARE @FinancialYear NVARCHAR(9) = @YearCurrent + N'-' + @YearNext;
    DECLARE @NextNo INT;
    DECLARE @SeqText VARCHAR(10);
    DECLARE @Candidate NVARCHAR(50);
    DECLARE @LocalTran BIT = 0;

    BEGIN TRY
        IF @@TRANCOUNT = 0
        BEGIN
            BEGIN TRANSACTION;
            SET @LocalTran = 1;
        END;

        IF OBJECT_ID(N'dbo.DcNumberSequence', N'U') IS NULL
        BEGIN
            THROW 50010, 'DcNumberSequence is missing. Run DatabaseSetup_DcNumberRegistry.sql before generating DC numbers.', 1;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.DcNumberSequence WITH (UPDLOCK, HOLDLOCK)
            WHERE FinancialYear = @FinancialYear
              AND DocumentType = N'OUTWARD'
        )
        BEGIN
            INSERT INTO dbo.DcNumberSequence (FinancialYear, DocumentType, Prefix, LastAllocatedSeq)
            VALUES (@FinancialYear, N'OUTWARD', N'SSE-', 1013);
        END;

        UPDATE dbo.DcNumberSequence WITH (UPDLOCK, HOLDLOCK)
        SET LastAllocatedSeq = LastAllocatedSeq + 1,
            UpdatedOn = GETDATE()
        WHERE FinancialYear = @FinancialYear
          AND DocumentType = N'OUTWARD';

        SELECT @NextNo = LastAllocatedSeq
        FROM dbo.DcNumberSequence
        WHERE FinancialYear = @FinancialYear
          AND DocumentType = N'OUTWARD';

        SET @SeqText = CASE WHEN @NextNo < 10000 THEN RIGHT('0000' + CAST(@NextNo AS VARCHAR(10)), 4) ELSE CAST(@NextNo AS VARCHAR(10)) END;
        SET @Candidate = CONCAT(N'SSE-', @SeqText, N'/', @FinancialYear);

        WHILE EXISTS (SELECT 1 FROM dbo.DcNumberRegistry WHERE DcNo = @Candidate)
           OR EXISTS (SELECT 1 FROM dbo.Outward WHERE OutwardDcNo = @Candidate)
        BEGIN
            SET @NextNo = @NextNo + 1;
            SET @SeqText = CASE WHEN @NextNo < 10000 THEN RIGHT('0000' + CAST(@NextNo AS VARCHAR(10)), 4) ELSE CAST(@NextNo AS VARCHAR(10)) END;
            SET @Candidate = CONCAT(N'SSE-', @SeqText, N'/', @FinancialYear);
        END;

        UPDATE dbo.DcNumberSequence
        SET LastAllocatedSeq = @NextNo,
            UpdatedOn = GETDATE()
        WHERE FinancialYear = @FinancialYear
          AND DocumentType = N'OUTWARD';

        SET @OutwardDcNo = @Candidate;

        INSERT INTO dbo.DcNumberRegistry
        (
            DcNo,
            SequenceNo,
            FinancialYear,
            DocumentType,
            CompanyId,
            Status,
            CreatedBy
        )
        VALUES
        (
            @OutwardDcNo,
            @NextNo,
            @FinancialYear,
            N'OUTWARD',
            @CompanyId,
            N'Allocated',
            @CreatedBy
        );

        SET @AllocationId = CONVERT(BIGINT, SCOPE_IDENTITY());

        INSERT INTO dbo.DcNumberAudit (AllocationId, DcNo, EventType, Actor, Details)
        VALUES (@AllocationId, @OutwardDcNo, N'Allocated', @CreatedBy, N'New number from persistent sequence.');

        IF @LocalTran = 1
            COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @LocalTran = 1 AND @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO
