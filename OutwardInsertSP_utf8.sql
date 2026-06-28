
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


