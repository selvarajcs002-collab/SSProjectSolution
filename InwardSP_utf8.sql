
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
END

