-- =============================================
-- Author:      Antigravity
-- Create date: 2026-04-03
-- Update date: 2026-04-03 (COMPLETE ENTERPRISE VERSION)
-- Description: SQL Script to create Tables and Stored Procedures for SSManagement
-- Server:      DESKTOP-U0IT7FS\SQLEXPRESS
-- Database:    SSManagement
-- =============================================

USE [SSManagement];
GO

-- 1. Create Table: Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        userId INT PRIMARY KEY IDENTITY(1,1),
        email NVARCHAR(150),
        password NVARCHAR(250),
        createdBy NVARCHAR(100),
        createdDate DATETIME DEFAULT GETDATE(),
        updatedDate DATETIME NULL
    );
END
GO

-- 2. Create Table: CompanyDetails
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CompanyDetails')
BEGIN
    CREATE TABLE CompanyDetails (
        companyId INT PRIMARY KEY IDENTITY(1,1),
        companyName NVARCHAR(200),
        gst_no NVARCHAR(50),
        phoneNumber NVARCHAR(20),
        door_no NVARCHAR(50),
        street_Name NVARCHAR(150),
        landmark NVARCHAR(150),
        city NVARCHAR(100),
        pincode NVARCHAR(20)
    );
END
GO

-- 3. Stored Procedure: User
IF OBJECT_ID('sp_ManageUser', 'P') IS NOT NULL
    DROP PROCEDURE sp_ManageUser;
GO

CREATE PROCEDURE sp_ManageUser
    @mode VARCHAR(10),
    @userId INT = NULL,
    @email NVARCHAR(150),
    @password NVARCHAR(250),
    @createdBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @mode = 'INSERT'
    BEGIN
        INSERT INTO Users (email, password, createdBy, createdDate)
        VALUES (@email, @password, @createdBy, GETDATE());

        SELECT SCOPE_IDENTITY() AS id, 'User Created Successfully' AS message, CAST(1 AS BIT) AS status;
    END

    ELSE IF @mode = 'UPDATE'
    BEGIN
        IF @userId IS NULL
        BEGIN
            SELECT 0 AS id, 'UserId is required' AS message, CAST(0 AS BIT) AS status;
            RETURN;
        END

        UPDATE Users
        SET email = @email,
            password = @password,
            updatedDate = GETDATE()
        WHERE userId = @userId;

        SELECT @userId AS id, 'User Updated Successfully' AS message, CAST(1 AS BIT) AS status;
    END
END
GO

-- 4. Stored Procedure: Company
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
    @pincode NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF @mode = 'INSERT'
    BEGIN
        INSERT INTO CompanyDetails
        (companyName, gst_no, phoneNumber, door_no, street_Name, landmark, city, pincode)
        VALUES
        (@companyName, @gst_no, @phoneNumber, @door_no, @street_Name, @landmark, @city, @pincode);

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
            pincode = @pincode
        WHERE companyId = @companyId;

        SELECT @companyId AS id, 'Company Updated Successfully' AS message, CAST(1 AS BIT) AS status;
    END
END
GO

-- 5. Stored Procedure: Validate User (Login)
IF OBJECT_ID('sp_LoginUser', 'P') IS NOT NULL
    DROP PROCEDURE sp_LoginUser;
GO

CREATE PROCEDURE sp_LoginUser
    @email NVARCHAR(150),
    @password NVARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validation
    IF (@email IS NULL OR @password IS NULL)
    BEGIN
        SELECT 
            0 AS id, 
            'Email and Password are required' AS message, 
            CAST(0 AS BIT) AS status;
        RETURN;
    END

    -- Check User
    IF EXISTS (
        SELECT 1 
        FROM Users 
        WHERE email = @email AND password = @password
    )
    BEGIN
        SELECT 
            userId AS id, 
            'Login Successful' AS message, 
            CAST(1 AS BIT) AS status
        FROM Users
        WHERE email = @email AND password = @password;
    END
    ELSE
    BEGIN
        SELECT 
            0 AS id, 
            'Invalid Email or Password' AS message, 
            CAST(0 AS BIT) AS status;
    END
END
GO

-- 6. Stored Procedure: Get Company List (Key-Value)
IF OBJECT_ID('sp_GetCompanyList', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetCompanyList;
GO

CREATE PROCEDURE sp_GetCompanyList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        companyId AS [key],
        companyName AS [value]
    FROM CompanyDetails
    ORDER BY companyName;
END
GO

-- 7. Stored Procedure: Get Company By ID
IF OBJECT_ID('sp_GetCompanyById', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetCompanyById;
GO

CREATE PROCEDURE sp_GetCompanyById
    @companyId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validation
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
            '' AS pincode;
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
        pincode
    FROM CompanyDetails
    WHERE companyId = @companyId;
END
GO
-- =============================================
-- Author:      Antigravity
-- Create date: 2026-04-04
-- Update date: 2026-04-04 (COMPLETE RE-IMPLEMENTATION)
-- Description: SQL Script to create Inward, InwardSizeCount tables and UDTT
-- Database:    SSManagement
-- =============================================

USE [SSManagement];
GO

-- 1. Create User Defined Table Type: SizeCountType
IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'SizeCountType')
BEGIN
    CREATE TYPE SizeCountType AS TABLE (
        Size NVARCHAR(10),
        Count INT
    );
END
GO

-- 2. Create Table: Inward
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Inward')
BEGIN
    CREATE TABLE Inward (
        InwardId INT PRIMARY KEY IDENTITY(1,1),
        CompanyId INT NOT NULL,
        Colour NVARCHAR(100) NOT NULL,
        DesignName NVARCHAR(150) NOT NULL,
        StyleNo NVARCHAR(100) NOT NULL,
        UploadURL NVARCHAR(500) NULL,
        CreatedBy INT NOT NULL,
        CreatedDate DATETIME DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        CONSTRAINT FK_Inward_Company FOREIGN KEY (CompanyId) REFERENCES CompanyDetails(companyId)
    );
END
GO

-- Alter Table: Inward to add InwardDcNo (Must happen before procedures are created)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Inward') AND name = 'InwardDcNo')
BEGIN
    ALTER TABLE Inward ADD InwardDcNo NVARCHAR(100) NULL;
END
GO

-- 3. Create Table: InwardSizeCount
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InwardSizeCount')
BEGIN
    CREATE TABLE InwardSizeCount (
        Id INT PRIMARY KEY IDENTITY(1,1),
        InwardId INT NOT NULL,
        StyleNo NVARCHAR(100) NOT NULL,
        DesignName NVARCHAR(150) NOT NULL,
        Colour NVARCHAR(100) NOT NULL,
        Size NVARCHAR(10) NOT NULL,
        Count INT NOT NULL CHECK (Count >= 0),
        CONSTRAINT FK_InwardSizeCount_Inward FOREIGN KEY (InwardId) REFERENCES Inward(InwardId) ON DELETE CASCADE
    );
END
GO

-- 4. Stored Procedure: Insert Inward Header
IF OBJECT_ID('sp_InsertInward', 'P') IS NOT NULL
    DROP PROCEDURE sp_InsertInward;
GO

CREATE PROCEDURE sp_InsertInward
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @DesignName NVARCHAR(150),
    @StyleNo NVARCHAR(100),
    @InwardDcNo NVARCHAR(100),
    @UploadURL NVARCHAR(500) = NULL,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Trim strings
    SET @Colour = LTRIM(RTRIM(@Colour));
    SET @DesignName = LTRIM(RTRIM(@DesignName));
    SET @StyleNo = LTRIM(RTRIM(@StyleNo));
    SET @InwardDcNo = LTRIM(RTRIM(@InwardDcNo));

    INSERT INTO Inward (
        CompanyId, 
        Colour, 
        DesignName, 
        StyleNo, 
        InwardDcNo,
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
        @UploadURL, 
        @CreatedBy, 
        GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS InwardId;
END
GO

-- 5. Stored Procedure: Insert Inward Size Counts (Bulk mapping)
IF OBJECT_ID('sp_InsertInwardSizeCounts', 'P') IS NOT NULL
    DROP PROCEDURE sp_InsertInwardSizeCounts;
GO

CREATE PROCEDURE sp_InsertInwardSizeCounts
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
GO


-- 7. Stored Procedure: Get Sizes By Colour and Style
IF OBJECT_ID('sp_GetSizes_ByColour_Style', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetSizes_ByColour_Style;
GO

CREATE PROCEDURE sp_GetSizes_ByColour_Style
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @StyleNo NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Size AS [size],
        [Count] AS [count]
    FROM InwardSizeCount
    INNER JOIN Inward ON InwardSizeCount.InwardId = Inward.InwardId
    WHERE Inward.CompanyId = @CompanyId
      AND Inward.Colour = @Colour
      AND Inward.StyleNo = @StyleNo;
END
GO

-- 8. Stored Procedure: Get Inward By Company and DC No
IF OBJECT_ID('sp_GetInward_ByCompany_And_DCNo', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetInward_ByCompany_And_DCNo;
GO

CREATE PROCEDURE sp_GetInward_ByCompany_And_DCNo
    @CompanyId INT,
    @InwardDcNo NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        InwardId AS inward_id,
        CompanyId AS company_id,
        Colour AS colour,
        DesignName AS design_name,
        StyleNo AS style_no,
        InwardDcNo AS inward_dc_no
    FROM Inward
    WHERE CompanyId = @CompanyId
      AND InwardDcNo = @InwardDcNo;
END
GO

-- 9. Stored Procedure: Update Inward
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
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Inward
    SET Colour = @Colour,
        DesignName = @DesignName,
        StyleNo = @StyleNo,
        InwardDcNo = @InwardDcNo,
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

-- 10. Stored Procedure: Get Design, Style and Colour By Company
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
        Colour AS colour
    FROM Inward
    WHERE CompanyId = @CompanyId;
END
GO
