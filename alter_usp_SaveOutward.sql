ALTER PROCEDURE [dbo].[usp_SaveOutward]
(
    @Mode NVARCHAR(10), -- 'INSERT' / 'UPDATE'
    @OutwardId INT = NULL OUTPUT,
    @CompanyId INT,
    @Colour NVARCHAR(50),
    @DesignName NVARCHAR(100),
    @StyleNo NVARCHAR(50),
    @UploadURL NVARCHAR(255),
    @CreatedBy NVARCHAR(100),
    @OutwardDcNo NVARCHAR(50) = NULL OUTPUT,
    @Status NVARCHAR(50),
    @SizeData NVARCHAR(MAX) -- JSON
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -----------------------------------------
        -- VALIDATE MODE
        -----------------------------------------
        IF (@Mode NOT IN ('INSERT', 'UPDATE'))
        BEGIN
            SELECT 0 AS Success, 'Invalid Mode' AS Message, NULL AS OutwardId, NULL AS OutwardDcNo;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -----------------------------------------
        -- EXTRACT SIZE DATA
        -----------------------------------------
        DECLARE @InputSizes TABLE
        (
            StyleNo NVARCHAR(50),
            DesignName NVARCHAR(100),
            Colour NVARCHAR(50),
            Size NVARCHAR(20),
            Count INT
        );

        -- Check if JSON contains multi-colour structure
        IF (ISJSON(@SizeData) = 1 AND JSON_QUERY(@SizeData, '$.colourBreakdowns') IS NOT NULL)
        BEGIN
            INSERT INTO @InputSizes (StyleNo, DesignName, Colour, Size, Count)
            SELECT 
                @StyleNo,
                @DesignName,
                LTRIM(RTRIM(c.Colour)),
                LTRIM(RTRIM(s.size)),
                s.[count]
            FROM OPENJSON(@SizeData, '$.colourBreakdowns')
            WITH (
                Colour NVARCHAR(50) '$.colour',
                sizes NVARCHAR(MAX) '$.sizes' AS JSON
            ) c
            CROSS APPLY OPENJSON(c.sizes)
            WITH (
                size NVARCHAR(20) '$.size',
                [count] INT '$.count'
            ) s
            WHERE s.[count] > 0;
            
            -- Keep first colour in main variable for legacy queries if not provided
            IF @Colour IS NULL OR @Colour = ''
            BEGIN
                SELECT TOP 1 @Colour = Colour FROM @InputSizes;
            END
        END
        ELSE IF (ISJSON(@SizeData) = 1 AND JSON_QUERY(@SizeData, '$.sizes') IS NOT NULL)
        BEGIN
            -- Legacy Single-Colour Structure
            INSERT INTO @InputSizes (StyleNo, DesignName, Colour, Size, Count)
            SELECT 
                @StyleNo,
                @DesignName,
                @Colour,
                Size,
                Count
            FROM OPENJSON(@SizeData, '$.sizes')
            WITH
            (
                Size NVARCHAR(20) '$.size',
                Count INT '$.count'
            )
            WHERE Count > 0;
        END

        IF NOT EXISTS (SELECT 1 FROM @InputSizes)
        BEGIN
            SELECT 0 AS Success, 'At least one colour and size must be provided with count > 0' AS Message, NULL AS OutwardId, NULL AS OutwardDcNo;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Validate Duplicate Colour+Size
        IF EXISTS (
            SELECT Colour, Size
            FROM @InputSizes
            GROUP BY Colour, Size
            HAVING COUNT(*) > 1
        )
        BEGIN
            SELECT 0 AS Success, 'Duplicate sizes found within a single colour.' AS Message, NULL AS OutwardId, NULL AS OutwardDcNo;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -----------------------------------------
        -- INWARD STOCK
        -----------------------------------------
        DECLARE @Inward TABLE
        (
            StyleNo NVARCHAR(50),
            DesignName NVARCHAR(100),
            Colour NVARCHAR(50),
            Size NVARCHAR(20),
            TotalInward INT
        );

        INSERT INTO @Inward
        SELECT 
            StyleNo, DesignName, Colour, Size,
            SUM([Count])
        FROM SSManagement.dbo.InwardSizeCount
        WHERE StyleNo = @StyleNo AND DesignName = @DesignName
        GROUP BY StyleNo, DesignName, Colour, Size;

        -----------------------------------------
        -- OUTWARD USED (IMPORTANT FOR UPDATE)
        -----------------------------------------
        DECLARE @OutwardUsed TABLE
        (
            StyleNo NVARCHAR(50),
            DesignName NVARCHAR(100),
            Colour NVARCHAR(50),
            Size NVARCHAR(20),
            TotalOutward INT
        );

        INSERT INTO @OutwardUsed
        SELECT 
            StyleNo, DesignName, Colour, Size,
            SUM([Count])
        FROM SSManagement.dbo.OutwardSizeCount
        WHERE (@Mode = 'INSERT' OR OutwardId <> @OutwardId)
          AND StyleNo = @StyleNo AND DesignName = @DesignName
        GROUP BY StyleNo, DesignName, Colour, Size;

        -----------------------------------------
        -- AVAILABLE STOCK
        -----------------------------------------
        DECLARE @Available TABLE
        (
            StyleNo NVARCHAR(50),
            DesignName NVARCHAR(100),
            Colour NVARCHAR(50),
            Size NVARCHAR(20),
            AvailableCount INT
        );

        INSERT INTO @Available
        SELECT 
            i.StyleNo,
            i.DesignName,
            i.Colour,
            i.Size,
            ISNULL(i.TotalInward,0) - ISNULL(o.TotalOutward,0)
        FROM @Inward i
        LEFT JOIN @OutwardUsed o
            ON i.StyleNo=o.StyleNo
            AND i.DesignName=o.DesignName
            AND i.Colour=o.Colour
            AND i.Size=o.Size;

        -----------------------------------------
        -- SIZE LEVEL VALIDATION
        -----------------------------------------
        DECLARE @ErrorMsg NVARCHAR(MAX);

        SELECT @ErrorMsg = STRING_AGG(
            'Colour ' + i.Colour + ' Size ' + i.Size +
            ' Available: ' + CAST(ISNULL(a.AvailableCount,0) AS VARCHAR) +
            ' Given: ' + CAST(i.Count AS VARCHAR),
            ' | '
        )
        FROM @InputSizes i
        LEFT JOIN @Available a
            ON i.StyleNo=a.StyleNo
            AND i.DesignName=a.DesignName
            AND i.Colour=a.Colour
            AND i.Size=a.Size
        WHERE i.Count > ISNULL(a.AvailableCount,0);

        IF (@ErrorMsg IS NOT NULL)
        BEGIN
            SELECT 0 AS Success, @ErrorMsg AS Message, NULL AS OutwardId, NULL AS OutwardDcNo;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -----------------------------------------
        -- INSERT MODE
        -----------------------------------------
        IF (@Mode = 'INSERT')
        BEGIN
            DECLARE @CompanyPrefix NVARCHAR(10), @MaxNo INT;

            SELECT @CompanyPrefix = UPPER(LEFT(CompanyName,3))
            FROM SSManagement.dbo.CompanyDetails
            WHERE CompanyId = @CompanyId;

            SELECT @MaxNo = ISNULL(MAX(CAST(SUBSTRING(OutwardDcNo, LEN(@CompanyPrefix) + 2, LEN(OutwardDcNo)) AS INT)), 0)
            FROM SSManagement.dbo.Outward
            WHERE OutwardDcNo LIKE @CompanyPrefix + '-%';

            SET @OutwardDcNo = @CompanyPrefix + '-' + CAST((@MaxNo + 1) AS NVARCHAR(50));

            INSERT INTO SSManagement.dbo.Outward 
                (CompanyId, Colour, DesignName, StyleNo, UploadURL, CreatedBy, OutwardDcNo, Status)
            VALUES 
                (@CompanyId, @Colour, @DesignName, @StyleNo, @UploadURL, @CreatedBy, @OutwardDcNo, @Status);

            SET @OutwardId = SCOPE_IDENTITY();

            -- Create OutwardColour entries
            INSERT INTO SSManagement.dbo.OutwardColour (OutwardId, Colour)
            SELECT DISTINCT @OutwardId, Colour
            FROM @InputSizes;

            -- Create OutwardSizeCount entries linking to OutwardColour
            INSERT INTO SSManagement.dbo.OutwardSizeCount (OutwardId, OutwardColourId, StyleNo, DesignName, Colour, Size, Count)
            SELECT 
                @OutwardId, 
                oc.OutwardColourId,
                i.StyleNo, 
                i.DesignName, 
                i.Colour, 
                i.Size, 
                i.Count
            FROM @InputSizes i
            JOIN SSManagement.dbo.OutwardColour oc ON oc.OutwardId = @OutwardId AND oc.Colour = i.Colour;

            SELECT 1 AS Success, 'Outward saved successfully' AS Message, @OutwardId, @OutwardDcNo;
        END
        -----------------------------------------
        -- UPDATE MODE
        -----------------------------------------
        ELSE IF (@Mode = 'UPDATE')
        BEGIN
            IF (@OutwardId IS NULL OR @OutwardId <= 0)
            BEGIN
                SELECT 0 AS Success, 'Invalid OutwardId for UPDATE' AS Message, NULL AS OutwardId, NULL AS OutwardDcNo;
                ROLLBACK TRANSACTION;
                RETURN;
            END

            SELECT @OutwardDcNo = OutwardDcNo
            FROM SSManagement.dbo.Outward
            WHERE OutwardId = @OutwardId;

            UPDATE SSManagement.dbo.Outward
            SET 
                CompanyId   = @CompanyId,
                Colour      = @Colour,
                DesignName  = @DesignName,
                StyleNo     = @StyleNo,
                UploadURL   = @UploadURL,
                Status      = @Status,
                UpdatedDate = GETDATE()
            WHERE OutwardId = @OutwardId;

            -- Handle Colours
            DELETE FROM SSManagement.dbo.OutwardSizeCount WHERE OutwardId = @OutwardId;
            DELETE FROM SSManagement.dbo.OutwardColour WHERE OutwardId = @OutwardId;

            -- Re-insert Colours
            INSERT INTO SSManagement.dbo.OutwardColour (OutwardId, Colour)
            SELECT DISTINCT @OutwardId, Colour
            FROM @InputSizes;

            -- Re-insert Sizes
            INSERT INTO SSManagement.dbo.OutwardSizeCount (OutwardId, OutwardColourId, StyleNo, DesignName, Colour, Size, Count)
            SELECT 
                @OutwardId, 
                oc.OutwardColourId,
                i.StyleNo, 
                i.DesignName, 
                i.Colour, 
                i.Size, 
                i.Count
            FROM @InputSizes i
            JOIN SSManagement.dbo.OutwardColour oc ON oc.OutwardId = @OutwardId AND oc.Colour = i.Colour;

            SELECT 1 AS Success, 'Outward updated successfully' AS Message, @OutwardId, @OutwardDcNo;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 0 AS Success, ERROR_MESSAGE() AS Message, NULL AS OutwardId, NULL AS OutwardDcNo;
    END CATCH
END;
