-- =============================================
-- Author:      Antigravity
-- Description: Update CompanyDetails and Inward to store DeliveryToLocations and PoNo
-- =============================================

USE [SSManagement];
GO

-- 1. Add DeliveryToLocations to CompanyDetails
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CompanyDetails') AND name = 'deliveryToLocations')
BEGIN
    ALTER TABLE CompanyDetails ADD deliveryToLocations NVARCHAR(MAX) NULL;
END
GO

-- 2. Add PoNo to Inward
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Inward') AND name = 'PoNo')
BEGIN
    ALTER TABLE Inward ADD PoNo NVARCHAR(100) NULL;
END
GO

-- 3. Update sp_ManageCompany
IF OBJECT_ID('sp_ManageCompany', 'P') IS NOT NULL
    DROP PROCEDURE sp_ManageCompany;
GO

CREATE PROCEDURE sp_ManageCompany
    @mode VARCHAR(10),
    @companyId INT = NULL,
    @companyName NVARCHAR(200),
    @gst_no NVARCHAR(50),
    @phoneNumber NVARCHAR(20),
    @door_no NVARCHAR(50),
    @street_Name NVARCHAR(150),
    @landmark NVARCHAR(150),
    @city NVARCHAR(100),
    @pincode NVARCHAR(20),
    @deliveryToLocations NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @mode = 'INSERT'
    BEGIN
        INSERT INTO CompanyDetails
        (companyName, gst_no, phoneNumber, door_no, street_Name, landmark, city, pincode, deliveryToLocations)
        VALUES
        (@companyName, @gst_no, @phoneNumber, @door_no, @street_Name, @landmark, @city, @pincode, @deliveryToLocations);

        SELECT SCOPE_IDENTITY() AS id, 'Company Created Successfully' AS message, CAST(1 AS BIT) AS status;
    END

    ELSE IF @mode = 'UPDATE'
    BEGIN
        IF @companyId IS NULL
        BEGIN
            SELECT 0 AS id, 'CompanyId is required' AS message, CAST(0 AS BIT) AS status;
            RETURN;
        END

        UPDATE CompanyDetails
        SET companyName = @companyName,
            gst_no = @gst_no,
            phoneNumber = @phoneNumber,
            door_no = @door_no,
            street_Name = @street_Name,
            landmark = @landmark,
            city = @city,
            pincode = @pincode,
            deliveryToLocations = @deliveryToLocations
        WHERE companyId = @companyId;

        SELECT @companyId AS id, 'Company Updated Successfully' AS message, CAST(1 AS BIT) AS status;
    END
END
GO

-- 4. Update sp_GetCompanyById
IF OBJECT_ID('sp_GetCompanyById', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetCompanyById;
GO

CREATE PROCEDURE sp_GetCompanyById
    @companyId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @companyId IS NULL OR @companyId = 0
    BEGIN
        SELECT 
            0 AS companyId,
            '' AS companyName,
            '' AS gst_no,
            '' AS phoneNumber,
            '' AS door_no,
            '' AS street_Name,
            '' AS landmark,
            '' AS city,
            '' AS pincode,
            NULL AS deliveryToLocations;
        RETURN;
    END

    SELECT 
        companyId,
        companyName,
        gst_no,
        phoneNumber,
        door_no,
        street_Name,
        landmark,
        city,
        pincode,
        deliveryToLocations
    FROM CompanyDetails
    WHERE companyId = @companyId;
END
GO

-- 5. Update sp_InsertInward
IF OBJECT_ID('sp_InsertInward', 'P') IS NOT NULL
    DROP PROCEDURE sp_InsertInward;
GO

CREATE PROCEDURE sp_InsertInward
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @DesignName NVARCHAR(150),
    @StyleNo NVARCHAR(100),
    @InwardDcNo NVARCHAR(100),
    @PoNo NVARCHAR(100) = NULL,
    @UploadURL NVARCHAR(500) = NULL,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Colour = LTRIM(RTRIM(@Colour));
    SET @DesignName = LTRIM(RTRIM(@DesignName));
    SET @StyleNo = LTRIM(RTRIM(@StyleNo));
    SET @InwardDcNo = LTRIM(RTRIM(@InwardDcNo));
    SET @PoNo = LTRIM(RTRIM(@PoNo));

    INSERT INTO Inward (
        CompanyId, 
        Colour, 
        DesignName, 
        StyleNo, 
        InwardDcNo,
        PoNo,
        UploadURL, 
        CreatedBy, 
        CreatedDate
    )
    VALUES (
        @CompanyId, 
        @Colour, 
        @DesignName, 
        @StyleNo, 
        @InwardDcNo,
        @PoNo,
        @UploadURL, 
        @CreatedBy, 
        GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS InwardId;
END
GO

-- 6. Update sp_UpdateInward
IF OBJECT_ID('sp_UpdateInward', 'P') IS NOT NULL
    DROP PROCEDURE sp_UpdateInward;
GO

CREATE PROCEDURE sp_UpdateInward
    @InwardId INT,
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @DesignName NVARCHAR(150),
    @StyleNo NVARCHAR(100),
    @InwardDcNo NVARCHAR(100),
    @PoNo NVARCHAR(100) = NULL,
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Inward
    SET Colour = @Colour,
        DesignName = @DesignName,
        StyleNo = @StyleNo,
        InwardDcNo = @InwardDcNo,
        PoNo = @PoNo,
        UpdatedDate = GETDATE()
    WHERE InwardId = @InwardId AND CompanyId = @CompanyId;

    IF @@ROWCOUNT > 0
    BEGIN
        SELECT 'Inward updated successfully' AS [message];
    END
    ELSE
    BEGIN
        SELECT 'Inward update failed or no changes made' AS [message];
    END
END
GO

-- 7. Update sp_GetDesignStyleColour_ByCompany
IF OBJECT_ID('sp_GetDesignStyleColour_ByCompany', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetDesignStyleColour_ByCompany;
GO

CREATE PROCEDURE sp_GetDesignStyleColour_ByCompany
    @CompanyId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        InwardId AS inwardId,
        DesignName AS designName,
        StyleNo AS styleNo,
        Colour AS colour,
        PoNo AS poNo
    FROM Inward
    WHERE CompanyId = @CompanyId;
END
GO
