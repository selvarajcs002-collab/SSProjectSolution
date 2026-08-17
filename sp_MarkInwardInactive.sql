USE [SSManagement];
GO

CREATE OR ALTER PROCEDURE sp_MarkInwardInactive
    @CompanyId INT,
    @StyleNo NVARCHAR(100),
    @DesignName NVARCHAR(100),
    @Colour NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Inward
    SET Status = 'InActive'
    WHERE CompanyId = @CompanyId
      AND StyleNo = @StyleNo
      AND DesignName = @DesignName
      AND Colour = @Colour
      AND Status = 'Active';

    SELECT 1 AS success, 'Inward rows marked as InActive successfully.' AS message;
END;
GO
