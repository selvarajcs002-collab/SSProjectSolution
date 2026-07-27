-- =============================================
-- Create date: 2026-07-11
-- Description: Get Styles for Stock Filtering
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetStyles_ForStock]
(
    @CompanyId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        StyleNo AS Value,
        StyleNo AS DisplayText
    FROM (
        SELECT StyleNo FROM Inward WHERE CompanyId = @CompanyId AND Status <> 'Deleted'
        UNION
        SELECT StyleNo FROM Outward WHERE CompanyId = @CompanyId AND Status <> 'Deleted'
    ) T
    WHERE StyleNo IS NOT NULL AND RTRIM(LTRIM(StyleNo)) <> ''
    ORDER BY StyleNo;
END
GO

-- =============================================
-- Create date: 2026-07-11
-- Description: Get Designs for Stock Filtering
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetDesigns_ForStock]
(
    @CompanyId INT,
    @StyleNo NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        DesignName AS Value,
        DesignName AS DisplayText
    FROM (
        SELECT DesignName FROM Inward WHERE CompanyId = @CompanyId AND StyleNo = @StyleNo AND Status <> 'Deleted'
        UNION
        SELECT DesignName FROM Outward WHERE CompanyId = @CompanyId AND StyleNo = @StyleNo AND Status <> 'Deleted'
    ) T
    WHERE DesignName IS NOT NULL AND RTRIM(LTRIM(DesignName)) <> ''
    ORDER BY DesignName;
END
GO

-- =============================================
-- Create date: 2026-07-11
-- Description: Get Colours for Stock Filtering
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetColours_ForStock]
(
    @CompanyId INT,
    @StyleNo NVARCHAR(50),
    @DesignName NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        Colour AS Value,
        Colour AS DisplayText
    FROM (
        SELECT Colour FROM Inward 
        WHERE CompanyId = @CompanyId AND StyleNo = @StyleNo AND DesignName = @DesignName AND Status <> 'Deleted'
        
        UNION
        
        SELECT O.Colour FROM Outward O 
        WHERE O.CompanyId = @CompanyId AND O.StyleNo = @StyleNo AND O.DesignName = @DesignName AND O.Status <> 'Deleted'
        
        UNION
        
        SELECT OSC.Colour FROM OutwardSizeCount OSC 
        INNER JOIN Outward O ON O.OutwardId = OSC.OutwardId 
        WHERE O.CompanyId = @CompanyId AND O.StyleNo = @StyleNo AND O.DesignName = @DesignName AND O.Status <> 'Deleted'
        
        UNION
        
        SELECT OC.Colour FROM OutwardColour OC 
        INNER JOIN Outward O ON O.OutwardId = OC.OutwardId 
        WHERE O.CompanyId = @CompanyId AND O.StyleNo = @StyleNo AND O.DesignName = @DesignName AND O.Status <> 'Deleted'
    ) T
    WHERE Colour IS NOT NULL AND RTRIM(LTRIM(Colour)) <> ''
    ORDER BY Colour;
END
GO
