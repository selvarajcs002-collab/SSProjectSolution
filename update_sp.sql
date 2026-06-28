
CREATE OR ALTER PROCEDURE [dbo].[usp_GetDetails_ById_Mode]
    @Id INT,
    @Mode NVARCHAR(10) -- 'INWARD' or 'OUTWARD'
AS
BEGIN
    SET NOCOUNT ON;
    IF @Mode = 'INWARD'
    BEGIN
        SELECT 
            I.InwardId,
            I.CompanyId,
            UPPER(cmp.CompanyName) AS CompanyName,
            I.Colour,
            I.DesignName,
            I.StyleNo,
            I.UploadURL,
            I.CreatedBy,
            I.CreatedDate,
            I.UpdatedDate,
            I.InwardDcNo,
            I.Status,
            I.InwardEntryType AS EntryType,
            I.PoNo,
            ISC.Id AS SizeCountId,
            ISC.Size,
            ISC.Count,
            ISC.Colour AS SizeColour
        FROM dbo.Inward I
        LEFT JOIN dbo.InwardSizeCount ISC
            ON I.InwardId = ISC.InwardId
        LEFT JOIN dbo.CompanyDetails cmp
            ON I.CompanyId = cmp.CompanyId
        WHERE I.InwardId = @Id
    END
    ELSE IF @Mode = 'OUTWARD'
    BEGIN
        SELECT 
            O.OutwardId,
            UPPER(cmp.CompanyName) AS CompanyName,
            O.CompanyId,
            O.Colour,
            O.DesignName,
            O.StyleNo,
            O.UploadURL,
            O.CreatedBy,
            O.CreatedDate,
            O.UpdatedDate,
            O.OutwardDcNo,
            O.Status,
            O.PoNo,
            O.SelectedDcNos,
            OSC.Id AS SizeCountId,
            OSC.Size,
            OSC.Count,
            OSC.Colour AS SizeColour
        FROM dbo.Outward O
        LEFT JOIN dbo.OutwardSizeCount OSC
            ON O.OutwardId = OSC.OutwardId
        LEFT JOIN dbo.CompanyDetails cmp
            ON O.CompanyId = cmp.CompanyId
        WHERE O.OutwardId = @Id
    END
    ELSE
    BEGIN
        SELECT 'Invalid Mode. Use INWARD or OUTWARD' AS Message
    END
END
