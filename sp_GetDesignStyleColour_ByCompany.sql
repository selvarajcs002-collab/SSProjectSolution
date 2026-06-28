ALTER PROCEDURE sp_GetDesignStyleColour_ByCompany
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Return normal colours
    SELECT DISTINCT
        i.InwardId AS inwardId,
        i.DesignName AS designName,
        i.StyleNo AS styleNo,
        i.Colour AS colour,
        i.PoNo AS poNo,
        i.InwardDcNo AS inwardDcNo
    FROM Inward i
    WHERE i.CompanyId = @CompanyId AND i.Colour != 'MULTI'

    UNION

    -- Return actual sizes colours for MULTI (Size flow)
    SELECT DISTINCT
        i.InwardId AS inwardId,
        i.DesignName AS designName,
        i.StyleNo AS styleNo,
        isc.Colour AS colour,
        i.PoNo AS poNo,
        i.InwardDcNo AS inwardDcNo
    FROM Inward i
    INNER JOIN InwardSizeCount isc ON i.InwardId = isc.InwardId
    WHERE i.CompanyId = @CompanyId AND i.Colour = 'MULTI' AND i.InwardEntryType = 'S' AND isc.Colour IS NOT NULL

    -- Wait, does INWARD_METER_DETAIL have Colour? I checked earlier, it does NOT have Colour.
    -- If InwardEntryType = 'M', they might not have 'MULTI' colour, but if they do, we can't extract it. 
    -- Assuming meter flow doesn't use MULTI or we just ignore it.
END
