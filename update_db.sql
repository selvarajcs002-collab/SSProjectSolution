
-- 1. Alter the actual tables to accept larger strings
ALTER TABLE InwardSizeCount ALTER COLUMN Size NVARCHAR(MAX);
ALTER TABLE OutwardSizeCount ALTER COLUMN Size NVARCHAR(MAX);

-- 2. Drop the dependent stored procedures
DROP PROCEDURE IF EXISTS [dbo].[sp_InsertInwardSizeCounts];
DROP PROCEDURE IF EXISTS [dbo].[sp_InsertOutwardSizeCounts];
DROP PROCEDURE IF EXISTS [dbo].[sp_UpdateOutwardSizeCounts];

-- 3. Drop the table types
DROP TYPE IF EXISTS [dbo].[SizeCountType];
DROP TYPE IF EXISTS [dbo].[OutwardSizeCountType];

-- 4. Recreate the table types with NVARCHAR(MAX)
CREATE TYPE [dbo].[SizeCountType] AS TABLE(
    [Size] [nvarchar](MAX) NULL,
    [Count] [int] NOT NULL
);

CREATE TYPE [dbo].[OutwardSizeCountType] AS TABLE(
    [OutwardColourId] [int] NULL,
    [Colour] [nvarchar](MAX) NULL,
    [Size] [nvarchar](MAX) NULL,
    [Count] [int] NULL
);
GO

-- 5. Recreate the stored procedures (using exact text from earlier, just executed again)
CREATE PROCEDURE [dbo].[sp_InsertInwardSizeCounts]
    @InwardId INT,
    @StyleNo NVARCHAR(100),
    @DesignName NVARCHAR(150),
    @Colour NVARCHAR(100),
    @SizeCounts SizeCountType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    -- Trim strings
    SET @StyleNo = LTRIM(RTRIM(@StyleNo));
    SET @DesignName = LTRIM(RTRIM(@DesignName));
    SET @Colour = LTRIM(RTRIM(@Colour));

    -- Insert multiple rows into InwardSizeCount
    INSERT INTO InwardSizeCount (
        InwardId, 
        StyleNo, 
        DesignName, 
        Colour, 
        Size, 
        Count
    )
    SELECT 
        @InwardId, 
        @StyleNo, 
        @DesignName, 
        @Colour, 
        LTRIM(RTRIM(Size)), 
        Count
    FROM @SizeCounts
    WHERE Size IS NOT NULL AND LTRIM(RTRIM(Size)) <> '';
END;
GO

CREATE PROCEDURE [dbo].[sp_InsertOutwardSizeCounts]
    @OutwardId INT,
    @SizeCounts OutwardSizeCountType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO OutwardSizeCount (OutwardId, OutwardColourId, Colour, Size, Count)
    SELECT @OutwardId, OutwardColourId, LTRIM(RTRIM(Colour)), LTRIM(RTRIM(Size)), Count
    FROM @SizeCounts
    WHERE Size IS NOT NULL AND LTRIM(RTRIM(Size)) <> '';
END;
GO

CREATE PROCEDURE [dbo].[sp_UpdateOutwardSizeCounts]
    @OutwardId INT,
    @SizeCounts OutwardSizeCountType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM OutwardSizeCount WHERE OutwardId = @OutwardId;
    INSERT INTO OutwardSizeCount (OutwardId, OutwardColourId, Colour, Size, Count)
    SELECT @OutwardId, OutwardColourId, LTRIM(RTRIM(Colour)), LTRIM(RTRIM(Size)), Count
    FROM @SizeCounts
    WHERE Size IS NOT NULL AND LTRIM(RTRIM(Size)) <> '';
END;
GO
