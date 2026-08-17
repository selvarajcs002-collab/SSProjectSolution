CREATE OR ALTER PROCEDURE sp_GetDesignStyleColour_ByCompany 
    @CompanyId INT 
AS 
BEGIN 
    SET NOCOUNT ON; 
    
    SELECT 
        inwardId, 
        designName, 
        styleNo, 
        colour, 
        poNo, 
        inwardDcNo, 
        status
    FROM (
        SELECT 
            i.InwardId AS inwardId, 
            i.DesignName AS designName, 
            i.StyleNo AS styleNo, 
            i.Colour AS colour, 
            i.PoNo AS poNo, 
            i.InwardDcNo AS inwardDcNo, 
            i.Status AS status,
            i.CreatedDate
        FROM Inward i 
        WHERE i.CompanyId = @CompanyId AND i.Colour != 'MULTI' AND i.Status = 'Active' 
        
        UNION ALL 
        
        SELECT 
            i.InwardId AS inwardId, 
            i.DesignName AS designName, 
            i.StyleNo AS styleNo, 
            isc.Colour AS colour, 
            i.PoNo AS poNo, 
            i.InwardDcNo AS inwardDcNo, 
            i.Status AS status,
            i.CreatedDate
        FROM Inward i 
        INNER JOIN InwardSizeCount isc ON i.InwardId = isc.InwardId 
        WHERE i.CompanyId = @CompanyId AND i.Colour = 'MULTI' AND i.InwardEntryType = 'S' AND isc.Colour IS NOT NULL AND i.Status = 'Active' 
    ) AS CombinedResults
    GROUP BY 
        inwardId, 
        designName, 
        styleNo, 
        colour, 
        poNo, 
        inwardDcNo, 
        status
    ORDER BY MAX(CreatedDate) DESC;
END
