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
