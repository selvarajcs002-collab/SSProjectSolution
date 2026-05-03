-- =============================================
-- Author:      Antigravity
-- Create date: 2026-04-04
-- Update date: 2026-04-04 (COMPLETE RE-IMPLEMENTATION)
-- Description: SQL Script to create Inward, InwardSizeCount tables and UDTT
-- Database:    SSManagement
-- =============================================


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
