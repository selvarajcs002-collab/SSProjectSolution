ALTER PROCEDURE [dbo].[sp_InsertInward]
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @DesignName NVARCHAR(150),
    @StyleNo NVARCHAR(100),
    @InwardDcNo NVARCHAR(100),
    @PoNo NVARCHAR(100) = NULL,
    @UploadURL NVARCHAR(500),
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Trim strings
    SET @Colour = LTRIM(RTRIM(@Colour));
    SET @DesignName = LTRIM(RTRIM(@DesignName));
    SET @StyleNo = LTRIM(RTRIM(@StyleNo));
    SET @InwardDcNo = LTRIM(RTRIM(@InwardDcNo));
    IF @PoNo IS NOT NULL
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

ALTER PROCEDURE [dbo].[sp_UpdateInward]
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

ALTER PROCEDURE [dbo].[usp_GetDetails_ById_Mode]
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
            I.UpdatedDate,
            I.InwardDcNo,
            I.PoNo,
            I.Status,

            ISC.Id AS SizeCountId,
            ISC.Size,
            ISC.Count,
            ISC.Colour AS SizeColour
        FROM SSManagement.dbo.Inward I
        LEFT JOIN SSManagement.dbo.InwardSizeCount ISC
            ON I.InwardId = ISC.InwardId
        LEFT JOIN SSManagement.dbo.CompanyDetails cmp
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
            O.Status,
            O.DeliveryTo,
            O.PoNo,
            O.Weight,
            O.NoOfBundles,
            OSC.Id AS SizeCountId,
            OSC.Size,
            OSC.Count,
            OSC.Colour AS SizeColour
        FROM SSManagement.dbo.Outward O
        LEFT JOIN SSManagement.dbo.OutwardSizeCount OSC
            ON O.OutwardId = OSC.OutwardId
        LEFT JOIN SSManagement.dbo.CompanyDetails cmp
            ON O.CompanyId = cmp.CompanyId
        WHERE O.OutwardId = @Id
    END
    ELSE
    BEGIN
        SELECT 'Invalid Mode. Use INWARD or OUTWARD' AS Message
    END
END
GO
