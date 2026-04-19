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
