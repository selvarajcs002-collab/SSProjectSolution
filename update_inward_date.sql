IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Inward]') AND name = 'InwardDate')
BEGIN
    ALTER TABLE [dbo].[Inward] ADD InwardDate DATETIME NULL;
END
GO

ALTER PROCEDURE [dbo].[sp_InsertInward]
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @DesignName NVARCHAR(150),
    @StyleNo NVARCHAR(100),
    @InwardDcNo NVARCHAR(100),
    @PoNo NVARCHAR(100) = NULL,
    @UploadURL NVARCHAR(500) = NULL,
    @CreatedBy INT,
    @InwardDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @DesignName = LTRIM(RTRIM(@DesignName));
    SET @StyleNo = LTRIM(RTRIM(@StyleNo));
    SET @InwardDcNo = LTRIM(RTRIM(@InwardDcNo));
    IF @PoNo IS NOT NULL SET @PoNo = LTRIM(RTRIM(@PoNo));

    INSERT INTO Inward (
        CompanyId, Colour, DesignName, StyleNo, InwardDcNo, PoNo, UploadURL, CreatedBy, CreatedDate, InwardDate
    )
    VALUES (
        @CompanyId, @Colour, @DesignName, @StyleNo, @InwardDcNo, @PoNo, @UploadURL, @CreatedBy, GETDATE(), @InwardDate
    );

    SELECT SCOPE_IDENTITY() AS InwardId;
END
GO

ALTER PROCEDURE [dbo].[sp_UpdateInward]
    @InwardId INT,
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @DesignName NVARCHAR(150),
    @StyleNo NVARCHAR(100),
    @InwardDcNo NVARCHAR(100),
    @PoNo NVARCHAR(100) = NULL,
    @UpdatedBy INT,
    @InwardDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Inward
    SET Colour = @Colour,
        DesignName = @DesignName,
        StyleNo = @StyleNo,
        InwardDcNo = @InwardDcNo,
        PoNo = @PoNo,
        UpdatedDate = GETDATE(),
        InwardDate = ISNULL(@InwardDate, InwardDate)
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

CREATE OR ALTER PROCEDURE [dbo].[sp_GetInward_ByCompany_And_DCNo]
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
        InwardDcNo AS inward_dc_no,
        COALESCE(InwardDate, CreatedDate) AS inward_date
    FROM Inward
    WHERE CompanyId = @CompanyId
      AND InwardDcNo = @InwardDcNo;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_GetDetails_ById_Mode]
(
    @Id INT,
    @Mode NVARCHAR(10) -- 'INWARD' / 'OUTWARD'
)
AS
BEGIN
    SET NOCOUNT ON;
    IF (@Mode = 'INWARD')
    BEGIN
        SELECT 
            I.InwardId,
            UPPER(cmp.CompanyName) AS CompanyName,
            I.CompanyId,
            I.Colour,
            I.DesignName,
            I.StyleNo,
            I.UploadURL,
            I.CreatedBy,
            I.CreatedDate,
            COALESCE(I.InwardDate, I.CreatedDate) AS InwardDate,
            I.UpdatedDate,
            I.InwardDcNo,
            I.PoNo,
            NULL AS DeliveryTo,
            NULL AS Weight,
            NULL AS NoOfBundles,
            I.Status,

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
    ELSE IF (@Mode = 'OUTWARD')
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
            O.DeliveryTo,
            O.PoNo,
            O.Weight,
            O.NoOfBundles,
            O.Remarks,
            O.Status,
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
END
GO
