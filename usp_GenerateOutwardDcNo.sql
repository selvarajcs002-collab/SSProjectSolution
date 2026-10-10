-- Run this on the same database as https://api.ssmanagement-ui.com
-- (usually SSManagement). Do not leave it on SSManagementDEV unless that is the API catalog.
USE [SSManagement];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Next outward DC number comes from DcNumberSequence, not from MAX of live Outward rows.
-- Deleted numbers stay recorded and are not issued again by this procedure.
-- If setup was applied to a different database, this procedure creates the numbering
-- objects here so generate-dc-no does not return 400.
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
    DECLARE @Seed INT = 1013;
    DECLARE @LocalTran BIT = 0;

    IF OBJECT_ID(N'dbo.DcNumberSequence', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DcNumberSequence
        (
            SequenceId        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            FinancialYear     NVARCHAR(9)       NOT NULL,
            DocumentType      NVARCHAR(20)      NOT NULL CONSTRAINT DF_DcNumberSequence_DocType DEFAULT (N'OUTWARD'),
            Prefix            NVARCHAR(10)      NOT NULL CONSTRAINT DF_DcNumberSequence_Prefix DEFAULT (N'SSE-'),
            LastAllocatedSeq  INT               NOT NULL CONSTRAINT DF_DcNumberSequence_Last DEFAULT (1013),
            UpdatedOn         DATETIME          NOT NULL CONSTRAINT DF_DcNumberSequence_UpdatedOn DEFAULT (GETDATE()),
            CONSTRAINT UX_DcNumberSequence_Scope UNIQUE (FinancialYear, DocumentType)
        );
    END;

    IF OBJECT_ID(N'dbo.DcNumberRegistry', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DcNumberRegistry
        (
            AllocationId          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            DcNo                  NVARCHAR(50)        NOT NULL,
            SequenceNo            INT                 NOT NULL,
            FinancialYear         NVARCHAR(9)         NOT NULL,
            DocumentType          NVARCHAR(20)        NOT NULL CONSTRAINT DF_DcNumberRegistry_DocType DEFAULT (N'OUTWARD'),
            CompanyId             INT                 NULL,
            OriginalOutwardId     INT                 NULL,
            CurrentOutwardId      INT                 NULL,
            PreviousOutwardId     INT                 NULL,
            RelatedAllocationId   BIGINT              NULL,
            Status                NVARCHAR(20)        NOT NULL,
            AllocatedOn           DATETIME            NOT NULL CONSTRAINT DF_DcNumberRegistry_AllocatedOn DEFAULT (GETDATE()),
            CreatedBy             NVARCHAR(100)       NULL,
            DeletedBy             NVARCHAR(100)       NULL,
            DeletedOn             DATETIME            NULL,
            DeletionReason        NVARCHAR(500)       NULL,
            ReusedBy              NVARCHAR(100)       NULL,
            ReusedOn              DATETIME            NULL,
            ReuseReason           NVARCHAR(500)       NULL,
            CONSTRAINT UX_DcNumberRegistry_DcNo UNIQUE (DcNo),
            CONSTRAINT CK_DcNumberRegistry_Status CHECK (Status IN (N'Allocated', N'Active', N'Deleted', N'Reserved', N'Reused'))
        );

        CREATE INDEX IX_DcNumberRegistry_StatusFy
            ON dbo.DcNumberRegistry (Status, FinancialYear, DocumentType);

        CREATE INDEX IX_DcNumberRegistry_CurrentOutward
            ON dbo.DcNumberRegistry (CurrentOutwardId);
    END;

    IF OBJECT_ID(N'dbo.DcNumberAudit', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DcNumberAudit
        (
            EventId        BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            AllocationId   BIGINT              NOT NULL,
            DcNo           NVARCHAR(50)        NOT NULL,
            EventType      NVARCHAR(30)        NOT NULL,
            Actor          NVARCHAR(100)       NULL,
            EventOn        DATETIME            NOT NULL CONSTRAINT DF_DcNumberAudit_EventOn DEFAULT (GETDATE()),
            Reason         NVARCHAR(500)       NULL,
            Details        NVARCHAR(MAX)       NULL,
            CONSTRAINT FK_DcNumberAudit_Registry FOREIGN KEY (AllocationId)
                REFERENCES dbo.DcNumberRegistry (AllocationId)
        );

        CREATE INDEX IX_DcNumberAudit_DcNo
            ON dbo.DcNumberAudit (DcNo, EventOn);
    END;

    BEGIN TRY
        IF @@TRANCOUNT = 0
        BEGIN
            BEGIN TRANSACTION;
            SET @LocalTran = 1;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.DcNumberSequence WITH (UPDLOCK, HOLDLOCK)
            WHERE FinancialYear = @FinancialYear
              AND DocumentType = N'OUTWARD'
        )
        BEGIN
            SELECT @Seed = ISNULL(MAX(v.SeqNo), 1013)
            FROM
            (
                SELECT TRY_CAST(SUBSTRING(LTRIM(RTRIM(o.OutwardDcNo)), 5, CHARINDEX('/', LTRIM(RTRIM(o.OutwardDcNo))) - 5) AS INT) AS SeqNo
                FROM dbo.Outward o
                WHERE o.OutwardDcNo LIKE N'SSE-%/%'
                  AND CHARINDEX('/', o.OutwardDcNo) > 5
                  AND SUBSTRING(o.OutwardDcNo, CHARINDEX('/', o.OutwardDcNo) + 1, 9) = @FinancialYear
                UNION ALL
                SELECT r.SequenceNo
                FROM dbo.DcNumberRegistry r
                WHERE r.FinancialYear = @FinancialYear
                  AND r.DocumentType = N'OUTWARD'
            ) v;

            INSERT INTO dbo.DcNumberSequence (FinancialYear, DocumentType, Prefix, LastAllocatedSeq)
            VALUES (@FinancialYear, N'OUTWARD', N'SSE-', @Seed);
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
