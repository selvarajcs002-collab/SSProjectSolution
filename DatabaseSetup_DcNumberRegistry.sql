USE [SSManagementDEV];
GO

/*
    Persistent outward DC numbering.

    Format is unchanged: SSE-####/YYYY-YYYY (example SSE-1014/2026-2027).
    Scope is financial year + document type OUTWARD. It is not per company.

    Normal create always increments DcNumberSequence.LastAllocatedSeq.
    Deleted numbers stay in DcNumberRegistry and are never issued again
    unless an authorized user explicitly reuses one.

    Rollback:
      DROP PROCEDURE IF EXISTS dbo.usp_ConfirmOutwardDcAllocation;
      DROP PROCEDURE IF EXISTS dbo.usp_GetReusableOutwardDcNos;
      DROP PROCEDURE IF EXISTS dbo.usp_ReserveReusedOutwardDcNo;
      DROP PROCEDURE IF EXISTS dbo.usp_MarkOutwardDcDeleted;
      -- restore previous usp_GenerateOutwardDcNo from source control
      DROP TABLE IF EXISTS dbo.DcNumberAudit;
      DROP TABLE IF EXISTS dbo.DcNumberRegistry;
      DROP TABLE IF EXISTS dbo.DcNumberSequence;
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

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
END
GO

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
END
GO

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
END
GO

-- Seed registry from live outwards. One row per DC number.
-- Existing data can have the same OutwardDcNo on more than one Outward row
-- (SSE-0806/2026-2027 is one example). NOT EXISTS against the empty
-- registry does not collapse those duplicates inside a single INSERT.
;WITH LiveIssued AS
(
    SELECT
        LTRIM(RTRIM(o.OutwardDcNo)) AS DcNo,
        o.CompanyId,
        o.OutwardId,
        o.CreatedDate,
        o.CreatedBy,
        ROW_NUMBER() OVER (
            PARTITION BY LTRIM(RTRIM(o.OutwardDcNo))
            ORDER BY o.OutwardId DESC
        ) AS rn
    FROM dbo.Outward o
    WHERE o.OutwardDcNo LIKE N'SSE-%/%'
      AND CHARINDEX('/', o.OutwardDcNo) > 5
      AND TRY_CAST(SUBSTRING(o.OutwardDcNo, 5, CHARINDEX('/', o.OutwardDcNo) - 5) AS INT) IS NOT NULL
)
INSERT INTO dbo.DcNumberRegistry
(
    DcNo, SequenceNo, FinancialYear, DocumentType, CompanyId,
    OriginalOutwardId, CurrentOutwardId, Status, AllocatedOn, CreatedBy
)
SELECT
    i.DcNo,
    TRY_CAST(SUBSTRING(i.DcNo, 5, CHARINDEX('/', i.DcNo) - 5) AS INT),
    SUBSTRING(i.DcNo, CHARINDEX('/', i.DcNo) + 1, 9),
    N'OUTWARD',
    i.CompanyId,
    i.OutwardId,
    i.OutwardId,
    N'Active',
    ISNULL(i.CreatedDate, GETDATE()),
    i.CreatedBy
FROM LiveIssued i
WHERE i.rn = 1
  AND NOT EXISTS (SELECT 1 FROM dbo.DcNumberRegistry r WHERE r.DcNo = i.DcNo);
GO

IF OBJECT_ID(N'dbo.OutwardArchive', N'U') IS NOT NULL
BEGIN
    ;WITH ArchivedIssued AS
    (
        SELECT
            LTRIM(RTRIM(a.OutwardDcNo)) AS DcNo,
            a.CompanyId,
            a.OutwardId,
            a.CreatedDate,
            a.CreatedBy,
            a.DeletedOn,
            ROW_NUMBER() OVER (
                PARTITION BY LTRIM(RTRIM(a.OutwardDcNo))
                ORDER BY a.OutwardId DESC
            ) AS rn
        FROM dbo.OutwardArchive a
        WHERE a.OutwardDcNo LIKE N'SSE-%/%'
          AND CHARINDEX('/', a.OutwardDcNo) > 5
          AND TRY_CAST(SUBSTRING(a.OutwardDcNo, 5, CHARINDEX('/', a.OutwardDcNo) - 5) AS INT) IS NOT NULL
    )
    INSERT INTO dbo.DcNumberRegistry
    (
        DcNo, SequenceNo, FinancialYear, DocumentType, CompanyId,
        OriginalOutwardId, PreviousOutwardId, Status, AllocatedOn, CreatedBy, DeletedOn
    )
    SELECT
        i.DcNo,
        TRY_CAST(SUBSTRING(i.DcNo, 5, CHARINDEX('/', i.DcNo) - 5) AS INT),
        SUBSTRING(i.DcNo, CHARINDEX('/', i.DcNo) + 1, 9),
        N'OUTWARD',
        i.CompanyId,
        i.OutwardId,
        i.OutwardId,
        N'Deleted',
        ISNULL(i.CreatedDate, GETDATE()),
        i.CreatedBy,
        i.DeletedOn
    FROM ArchivedIssued i
    WHERE i.rn = 1
      AND NOT EXISTS (SELECT 1 FROM dbo.DcNumberRegistry r WHERE r.DcNo = i.DcNo);
END
GO

INSERT INTO dbo.DcNumberSequence (FinancialYear, DocumentType, Prefix, LastAllocatedSeq)
SELECT
    r.FinancialYear,
    N'OUTWARD',
    N'SSE-',
    ISNULL(MAX(r.SequenceNo), 1013)
FROM dbo.DcNumberRegistry r
WHERE r.DocumentType = N'OUTWARD'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.DcNumberSequence s
      WHERE s.FinancialYear = r.FinancialYear
        AND s.DocumentType = N'OUTWARD'
  )
GROUP BY r.FinancialYear;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CompanyDetails_CompanyName' AND object_id = OBJECT_ID(N'dbo.CompanyDetails'))
BEGIN
    CREATE INDEX IX_CompanyDetails_CompanyName ON dbo.CompanyDetails (companyName);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.DcNumberSequence
    WHERE FinancialYear = CONCAT(CAST(YEAR(GETDATE()) AS NVARCHAR(4)), N'-', CAST(YEAR(GETDATE()) + 1 AS NVARCHAR(4)))
      AND DocumentType = N'OUTWARD'
)
BEGIN
    DECLARE @CurrentFy NVARCHAR(9) =
        CONCAT(CAST(YEAR(GETDATE()) AS NVARCHAR(4)), N'-', CAST(YEAR(GETDATE()) + 1 AS NVARCHAR(4)));

    DECLARE @Seed INT =
    (
        SELECT ISNULL(MAX(SequenceNo), 1013)
        FROM dbo.DcNumberRegistry
        WHERE FinancialYear = @CurrentFy
          AND DocumentType = N'OUTWARD'
    );

    INSERT INTO dbo.DcNumberSequence (FinancialYear, DocumentType, Prefix, LastAllocatedSeq)
    VALUES (@CurrentFy, N'OUTWARD', N'SSE-', @Seed);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GenerateOutwardDcNo
    @OutwardDcNo   NVARCHAR(50) OUTPUT,
    @CreatedBy     NVARCHAR(100) = NULL,
    @CompanyId     INT = NULL,
    @AllocationId  BIGINT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @YearCurrent NVARCHAR(4) = CAST(YEAR(GETDATE()) AS NVARCHAR(4));
    DECLARE @YearNext    NVARCHAR(4) = CAST(YEAR(GETDATE()) + 1 AS NVARCHAR(4));
    DECLARE @FinancialYear NVARCHAR(9) = @YearCurrent + N'-' + @YearNext;
    DECLARE @NextNo INT;
    DECLARE @SeqText VARCHAR(10);
    DECLARE @LocalTran BIT = 0;

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

        WHILE EXISTS (SELECT 1 FROM dbo.DcNumberRegistry WHERE SequenceNo = @NextNo AND FinancialYear = @FinancialYear)
           OR EXISTS (SELECT 1 FROM dbo.Outward WHERE OutwardDcNo = CONCAT(N'SSE-', CASE WHEN @NextNo < 10000 THEN RIGHT('0000' + CAST(@NextNo AS VARCHAR(10)), 4) ELSE CAST(@NextNo AS VARCHAR(10)) END, N'/', @FinancialYear))
        BEGIN
            SET @NextNo = @NextNo + 1;
        END;

        IF @NextNo <> (SELECT LastAllocatedSeq FROM dbo.DcNumberSequence WHERE FinancialYear = @FinancialYear AND DocumentType = N'OUTWARD')
        BEGIN
            UPDATE dbo.DcNumberSequence
            SET LastAllocatedSeq = @NextNo,
                UpdatedOn = GETDATE()
            WHERE FinancialYear = @FinancialYear
              AND DocumentType = N'OUTWARD';
        END;

        SET @SeqText = CASE WHEN @NextNo < 10000 THEN RIGHT('0000' + CAST(@NextNo AS VARCHAR(10)), 4) ELSE CAST(@NextNo AS VARCHAR(10)) END;
        SET @OutwardDcNo = CONCAT(N'SSE-', @SeqText, N'/', @FinancialYear);

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

CREATE OR ALTER PROCEDURE dbo.usp_ConfirmOutwardDcAllocation
    @OutwardDcNo NVARCHAR(50),
    @OutwardId   INT,
    @CompanyId   INT = NULL,
    @Actor       NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.DcNumberRegistry
    SET Status = CASE WHEN Status = N'Reserved' THEN N'Reused' ELSE N'Active' END,
        CurrentOutwardId = @OutwardId,
        OriginalOutwardId = ISNULL(OriginalOutwardId, @OutwardId),
        CompanyId = ISNULL(@CompanyId, CompanyId)
    WHERE DcNo = @OutwardDcNo;

    INSERT INTO dbo.DcNumberAudit (AllocationId, DcNo, EventType, Actor, Details)
    SELECT AllocationId, DcNo, N'Activated', @Actor, CONCAT(N'Bound to OutwardId ', @OutwardId)
    FROM dbo.DcNumberRegistry
    WHERE DcNo = @OutwardDcNo;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_MarkOutwardDcDeleted
    @OutwardId      INT,
    @OutwardDcNo    NVARCHAR(50),
    @DeletedBy      NVARCHAR(100) = NULL,
    @DeletionReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.DcNumberRegistry
    SET Status = N'Deleted',
        PreviousOutwardId = ISNULL(CurrentOutwardId, @OutwardId),
        CurrentOutwardId = NULL,
        DeletedBy = @DeletedBy,
        DeletedOn = GETDATE(),
        DeletionReason = @DeletionReason
    WHERE DcNo = @OutwardDcNo;

    IF @@ROWCOUNT = 0 AND NULLIF(LTRIM(RTRIM(@OutwardDcNo)), N'') IS NOT NULL
    BEGIN
        DECLARE @Seq INT = TRY_CAST(SUBSTRING(@OutwardDcNo, 5, CHARINDEX('/', @OutwardDcNo) - 5) AS INT);
        DECLARE @Fy NVARCHAR(9) = CASE WHEN CHARINDEX('/', @OutwardDcNo) > 0 THEN SUBSTRING(@OutwardDcNo, CHARINDEX('/', @OutwardDcNo) + 1, 9) ELSE NULL END;

        INSERT INTO dbo.DcNumberRegistry
        (
            DcNo, SequenceNo, FinancialYear, DocumentType, OriginalOutwardId, PreviousOutwardId,
            Status, DeletedBy, DeletedOn, DeletionReason
        )
        VALUES
        (
            @OutwardDcNo, ISNULL(@Seq, 0), ISNULL(@Fy, N''), N'OUTWARD', @OutwardId, @OutwardId,
            N'Deleted', @DeletedBy, GETDATE(), @DeletionReason
        );
    END;

    INSERT INTO dbo.DcNumberAudit (AllocationId, DcNo, EventType, Actor, Reason, Details)
    SELECT AllocationId, DcNo, N'Deleted', @DeletedBy, @DeletionReason, CONCAT(N'Archived OutwardId ', @OutwardId)
    FROM dbo.DcNumberRegistry
    WHERE DcNo = @OutwardDcNo;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetReusableOutwardDcNos
    @Search NVARCHAR(50) = NULL,
    @CompanyId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FinancialYear NVARCHAR(9) =
        CONCAT(CAST(YEAR(GETDATE()) AS NVARCHAR(4)), N'-', CAST(YEAR(GETDATE()) + 1 AS NVARCHAR(4)));

    SET @Search = NULLIF(LTRIM(RTRIM(@Search)), N'');

    SELECT
        r.AllocationId,
        r.DcNo,
        r.CompanyId,
        c.companyName AS CompanyName,
        r.OriginalOutwardId,
        r.PreviousOutwardId,
        r.AllocatedOn,
        r.DeletedOn,
        r.DeletionReason,
        r.FinancialYear,
        r.Status
    FROM dbo.DcNumberRegistry r
    LEFT JOIN dbo.CompanyDetails c ON c.companyId = r.CompanyId
    WHERE r.Status = N'Deleted'
      AND r.DocumentType = N'OUTWARD'
      AND r.FinancialYear = @FinancialYear
      AND NOT EXISTS (SELECT 1 FROM dbo.Outward o WHERE o.OutwardDcNo = r.DcNo)
      AND (@Search IS NULL OR r.DcNo LIKE N'%' + @Search + N'%')
      AND (@CompanyId IS NULL OR r.CompanyId = @CompanyId)
    ORDER BY r.SequenceNo;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReserveReusedOutwardDcNo
    @DcNo         NVARCHAR(50),
    @CompanyId    INT,
    @ReusedBy     NVARCHAR(100),
    @ReuseReason  NVARCHAR(500),
    @UserRole     NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF UPPER(LTRIM(RTRIM(ISNULL(@UserRole, N'')))) NOT IN (N'ADMINISTRATOR', N'ADMIN')
        BEGIN
            SELECT CAST(0 AS BIT) AS Success, N'You are not authorized to reuse a deleted DC number.' AS Message, NULL AS DcNo, NULL AS AllocationId;
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        IF NULLIF(LTRIM(RTRIM(@ReuseReason)), N'') IS NULL OR LEN(LTRIM(RTRIM(@ReuseReason))) < 3
        BEGIN
            SELECT CAST(0 AS BIT) AS Success, N'A reuse reason of at least 3 characters is required.' AS Message, NULL AS DcNo, NULL AS AllocationId;
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        DECLARE @FinancialYear NVARCHAR(9) =
            CONCAT(CAST(YEAR(GETDATE()) AS NVARCHAR(4)), N'-', CAST(YEAR(GETDATE()) + 1 AS NVARCHAR(4)));

        DECLARE @AllocationId BIGINT;
        DECLARE @Status NVARCHAR(20);
        DECLARE @RegistryFy NVARCHAR(9);

        SELECT
            @AllocationId = AllocationId,
            @Status = Status,
            @RegistryFy = FinancialYear
        FROM dbo.DcNumberRegistry WITH (UPDLOCK, HOLDLOCK)
        WHERE DcNo = @DcNo;

        IF @AllocationId IS NULL
        BEGIN
            SELECT CAST(0 AS BIT) AS Success, N'That DC number was never issued and cannot be reused.' AS Message, NULL AS DcNo, NULL AS AllocationId;
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        IF @Status <> N'Deleted'
        BEGIN
            SELECT CAST(0 AS BIT) AS Success, N'That DC number is not eligible for reuse.' AS Message, NULL AS DcNo, NULL AS AllocationId;
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        IF @RegistryFy <> @FinancialYear
        BEGIN
            SELECT CAST(0 AS BIT) AS Success, N'The requested DC number belongs to a different financial year.' AS Message, NULL AS DcNo, NULL AS AllocationId;
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        IF EXISTS (SELECT 1 FROM dbo.Outward WITH (UPDLOCK, HOLDLOCK) WHERE OutwardDcNo = @DcNo)
        BEGIN
            SELECT CAST(0 AS BIT) AS Success, N'That DC number is already used by an active outward.' AS Message, NULL AS DcNo, NULL AS AllocationId;
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        UPDATE dbo.DcNumberRegistry
        SET Status = N'Reserved',
            ReusedBy = @ReusedBy,
            ReusedOn = GETDATE(),
            ReuseReason = @ReuseReason,
            CompanyId = @CompanyId
        WHERE AllocationId = @AllocationId
          AND Status = N'Deleted';

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT CAST(0 AS BIT) AS Success, N'That DC number is already being reused by another request.' AS Message, NULL AS DcNo, NULL AS AllocationId;
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        INSERT INTO dbo.DcNumberAudit (AllocationId, DcNo, EventType, Actor, Reason, Details)
        VALUES (@AllocationId, @DcNo, N'Reserved', @ReusedBy, @ReuseReason, N'Authorized reuse of a deleted DC number. Original history is unchanged.');

        COMMIT TRANSACTION;

        SELECT CAST(1 AS BIT) AS Success, N'DC number reserved for reuse.' AS Message, @DcNo AS DcNo, @AllocationId AS AllocationId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT CAST(0 AS BIT) AS Success, ERROR_MESSAGE() AS Message, NULL AS DcNo, NULL AS AllocationId;
    END CATCH;
END;
GO
