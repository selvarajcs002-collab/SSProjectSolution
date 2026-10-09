USE [SSManagementDEV];
GO

/*
    Delete one outward from the Delivery Challan page.

    The live rows are copied into archive tables, then removed:
      OutwardSizeCount      -> OutwardSizeCountArchive
      OutwardColour         -> OutwardColourArchive
      OUTWARD_METER_DETAIL  -> OutwardMeterDetailArchive
      Outward               -> OutwardArchive

    Inward, InwardSizeCount, and INWARD_METER_DETAIL are not updated.
    Available balance comes back because those outward rows are no longer
    in the live tables.

    The outward DC number stays on OutwardArchive, so
    usp_GenerateOutwardDcNo does not issue that number again.
*/

CREATE OR ALTER PROCEDURE dbo.sp_DeleteOutward
    @OutwardId INT,
    @DeletedBy NVARCHAR(100) = NULL,
    @DeletionReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @OutwardId IS NULL OR @OutwardId <= 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT
                CAST(0 AS BIT) AS Success,
                'Invalid OutwardId.' AS Message,
                NULL AS OutwardId,
                NULL AS OutwardDcNo,
                0 AS RestoredBits,
                CAST(0 AS DECIMAL(18, 3)) AS RestoredMeter;
            RETURN;
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Outward WITH (UPDLOCK, HOLDLOCK)
            WHERE OutwardId = @OutwardId
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT
                CAST(0 AS BIT) AS Success,
                'Outward record was not found.' AS Message,
                @OutwardId AS OutwardId,
                NULL AS OutwardDcNo,
                0 AS RestoredBits,
                CAST(0 AS DECIMAL(18, 3)) AS RestoredMeter;
            RETURN;
        END;

        DECLARE @OutwardDcNo   NVARCHAR(50);
        DECLARE @EntryType     CHAR(1);
        DECLARE @RestoredBits  INT = 0;
        DECLARE @RestoredMeter DECIMAL(18, 3) = 0;

        SELECT
            @OutwardDcNo = OutwardDcNo,
            @EntryType   = OutwardEntryType
        FROM dbo.Outward
        WHERE OutwardId = @OutwardId;

        IF OBJECT_ID(N'dbo.OutwardSizeCount', N'U') IS NOT NULL
        BEGIN
            SELECT @RestoredBits = ISNULL(SUM(ISNULL([Count], 0)), 0)
            FROM dbo.OutwardSizeCount WITH (UPDLOCK, HOLDLOCK)
            WHERE OutwardId = @OutwardId;
        END;

        IF OBJECT_ID(N'dbo.OUTWARD_METER_DETAIL', N'U') IS NOT NULL
        BEGIN
            SELECT
                @RestoredBits = CASE
                    WHEN @EntryType = 'M' THEN ISNULL(SUM(ISNULL(OMD_BITS_COUNT, 0)), 0)
                    ELSE @RestoredBits
                END,
                @RestoredMeter = ISNULL(SUM(ISNULL(OMD_TOTAL_METER, 0)), 0)
            FROM dbo.OUTWARD_METER_DETAIL WITH (UPDLOCK, HOLDLOCK)
            WHERE OMD_OUTWARD_ID = @OutwardId;
        END;

        DECLARE @Jobs TABLE
        (
            Seq         INT          NOT NULL,
            LiveName    SYSNAME      NOT NULL,
            ArchiveName SYSNAME      NOT NULL,
            KeyColumn   SYSNAME      NOT NULL
        );

        INSERT INTO @Jobs (Seq, LiveName, ArchiveName, KeyColumn)
        VALUES
            (1, N'OutwardSizeCount',     N'OutwardSizeCountArchive',     N'OutwardId'),
            (2, N'OutwardColour',        N'OutwardColourArchive',        N'OutwardId'),
            (3, N'OUTWARD_METER_DETAIL', N'OutwardMeterDetailArchive',   N'OMD_OUTWARD_ID'),
            (4, N'Outward',              N'OutwardArchive',              N'OutwardId');

        DECLARE @Seq         INT;
        DECLARE @LiveName    SYSNAME;
        DECLARE @ArchiveName SYSNAME;
        DECLARE @KeyColumn   SYSNAME;
        DECLARE @Cols        NVARCHAR(MAX);
        DECLARE @Sql         NVARCHAR(MAX);
        DECLARE @HasIdentity BIT;

        DECLARE move_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT Seq, LiveName, ArchiveName, KeyColumn
            FROM @Jobs
            ORDER BY Seq;

        OPEN move_cursor;
        FETCH NEXT FROM move_cursor INTO @Seq, @LiveName, @ArchiveName, @KeyColumn;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF OBJECT_ID(N'dbo.' + @LiveName, N'U') IS NOT NULL
            BEGIN
                IF OBJECT_ID(N'dbo.' + @ArchiveName, N'U') IS NULL
                BEGIN
                    SET @Sql = N'
                        SELECT *
                        INTO dbo.' + QUOTENAME(@ArchiveName) + N'
                        FROM dbo.' + QUOTENAME(@LiveName) + N'
                        WHERE 1 = 0;

                        ALTER TABLE dbo.' + QUOTENAME(@ArchiveName) + N'
                        ADD DeletedOn DATETIME NULL;';

                    EXEC sys.sp_executesql @Sql;
                END;

                SELECT @Cols = STRING_AGG(QUOTENAME(c.name), N',') WITHIN GROUP (ORDER BY c.column_id)
                FROM sys.columns c
                WHERE c.object_id = OBJECT_ID(N'dbo.' + @LiveName)
                  AND c.is_computed = 0
                  AND TYPE_NAME(c.user_type_id) <> N'timestamp';

                SET @HasIdentity = CASE
                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM sys.identity_columns
                        WHERE object_id = OBJECT_ID(N'dbo.' + @ArchiveName)
                    ) THEN 1
                    ELSE 0
                END;

                SET @Sql = N'';

                IF @HasIdentity = 1
                    SET @Sql = N'SET IDENTITY_INSERT dbo.' + QUOTENAME(@ArchiveName) + N' ON; ';

                SET @Sql = @Sql + N'
                    INSERT INTO dbo.' + QUOTENAME(@ArchiveName) + N' (' + @Cols + N', DeletedOn)
                    SELECT ' + @Cols + N', GETDATE()
                    FROM dbo.' + QUOTENAME(@LiveName) + N' WITH (UPDLOCK, HOLDLOCK)
                    WHERE ' + QUOTENAME(@KeyColumn) + N' = @OutwardId;';

                IF @HasIdentity = 1
                    SET @Sql = @Sql + N' SET IDENTITY_INSERT dbo.' + QUOTENAME(@ArchiveName) + N' OFF;';

                EXEC sys.sp_executesql
                    @Sql,
                    N'@OutwardId INT',
                    @OutwardId = @OutwardId;
            END;

            FETCH NEXT FROM move_cursor INTO @Seq, @LiveName, @ArchiveName, @KeyColumn;
        END;

        CLOSE move_cursor;

        OPEN move_cursor;
        FETCH NEXT FROM move_cursor INTO @Seq, @LiveName, @ArchiveName, @KeyColumn;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            IF OBJECT_ID(N'dbo.' + @LiveName, N'U') IS NOT NULL
            BEGIN
                SET @Sql = N'
                    DELETE FROM dbo.' + QUOTENAME(@LiveName) + N'
                    WHERE ' + QUOTENAME(@KeyColumn) + N' = @OutwardId;';

                EXEC sys.sp_executesql
                    @Sql,
                    N'@OutwardId INT',
                    @OutwardId = @OutwardId;
            END;

            FETCH NEXT FROM move_cursor INTO @Seq, @LiveName, @ArchiveName, @KeyColumn;
        END;

        CLOSE move_cursor;
        DEALLOCATE move_cursor;

        IF OBJECT_ID(N'dbo.usp_MarkOutwardDcDeleted', N'P') IS NOT NULL
        BEGIN
            EXEC dbo.usp_MarkOutwardDcDeleted
                @OutwardId = @OutwardId,
                @OutwardDcNo = @OutwardDcNo,
                @DeletedBy = @DeletedBy,
                @DeletionReason = @DeletionReason;
        END;

        COMMIT TRANSACTION;

        SELECT
            CAST(1 AS BIT) AS Success,
            'Outward ' + ISNULL(@OutwardDcNo, N'')
                + ' moved to archive and removed. Inward total is unchanged. Available balance is restored.' AS Message,
            @OutwardId AS OutwardId,
            @OutwardDcNo AS OutwardDcNo,
            @RestoredBits AS RestoredBits,
            @RestoredMeter AS RestoredMeter;
    END TRY
    BEGIN CATCH
        IF CURSOR_STATUS('local', 'move_cursor') >= 0
        BEGIN
            CLOSE move_cursor;
            DEALLOCATE move_cursor;
        END
        ELSE IF CURSOR_STATUS('local', 'move_cursor') = -1
        BEGIN
            DEALLOCATE move_cursor;
        END;

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT
            CAST(0 AS BIT) AS Success,
            ERROR_MESSAGE() AS Message,
            @OutwardId AS OutwardId,
            NULL AS OutwardDcNo,
            0 AS RestoredBits,
            CAST(0 AS DECIMAL(18, 3)) AS RestoredMeter;
    END CATCH;
END;
GO
