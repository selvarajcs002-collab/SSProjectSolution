-- ========================================================================================
-- DATABASE SETUP SCRIPT FOR RATE QUOTATION MODULE
-- Contains Table and Stored Procedures
-- ========================================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'NoOfStitches')
    BEGIN
        ALTER TABLE [dbo].[RateQuotation] ADD 
            [NoOfStitches] INT NULL,
            [ChenilleColors] INT NULL,
            [NormalEmbColors] INT NULL;
    END
END
GO

-- 1. Create Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RateQuotation] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [QuotationNo] NVARCHAR(50) NOT NULL,
        [QuotationDate] DATETIME NOT NULL,
        [CompanyId] BIGINT NOT NULL,
        [CompanyName] NVARCHAR(200) NOT NULL,
        [ContactPerson] NVARCHAR(200) NULL,
        [MobileNo] NVARCHAR(20) NULL,
        [EmailId] NVARCHAR(200) NULL,
        [Address] NVARCHAR(MAX) NULL,
        [StyleNo] NVARCHAR(100) NULL,
        [DesignName] NVARCHAR(200) NULL,
        [ProductType] NVARCHAR(100) NULL,
        [RatePerPiece] DECIMAL(18,2) NULL,
        [RatePerMeter] DECIMAL(18,2) NULL,
        [NoOfStitches] INT NULL,
        [ChenilleColors] INT NULL,
        [NormalEmbColors] INT NULL,
        [Quantity] INT NOT NULL,
        [TotalAmount] DECIMAL(18,2) NOT NULL,
        [Remarks] NVARCHAR(MAX) NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'Draft',
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedBy] BIGINT NOT NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [ModifiedBy] BIGINT NULL,
        [ModifiedDate] DATETIME NULL,
        CONSTRAINT [PK_RateQuotation] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_RateQuotation_QuotationNo] ON [dbo].[RateQuotation]
    (
        [QuotationNo] ASC
    ) WHERE [IsActive] = 1;

    CREATE NONCLUSTERED INDEX [IX_RateQuotation_CompanyId] ON [dbo].[RateQuotation]
    (
        [CompanyId] ASC
    );
END
GO

-- 2. Stored Procedures

-- A. USP_RateQuotation_Insert
IF OBJECT_ID('USP_RateQuotation_Insert', 'P') IS NOT NULL
    DROP PROCEDURE USP_RateQuotation_Insert
GO
CREATE PROCEDURE [dbo].[USP_RateQuotation_Insert]
    @QuotationDate DATETIME,
    @CompanyId BIGINT,
    @CompanyName NVARCHAR(200),
    @ContactPerson NVARCHAR(200) = NULL,
    @MobileNo NVARCHAR(20) = NULL,
    @EmailId NVARCHAR(200) = NULL,
    @Address NVARCHAR(MAX) = NULL,
    @StyleNo NVARCHAR(100) = NULL,
    @DesignName NVARCHAR(200) = NULL,
    @ProductType NVARCHAR(100) = NULL,
    @RatePerPiece DECIMAL(18,2) = NULL,
    @RatePerMeter DECIMAL(18,2) = NULL,
    @NoOfStitches INT = NULL,
    @ChenilleColors INT = NULL,
    @NormalEmbColors INT = NULL,
    @Quantity INT,
    @TotalAmount DECIMAL(18,2),
    @Remarks NVARCHAR(MAX) = NULL,
    @Status NVARCHAR(20),
    @CreatedBy BIGINT,
    @NewId BIGINT OUTPUT,
    @StatusCode INT OUTPUT,
    @StatusMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Duplicate check removed because QuotationNo is auto-generated
        IF @CompanyId <= 0 OR ISNULL(@CompanyName, '') = ''
        BEGIN
            SET @StatusCode = 400;
            SET @StatusMessage = 'CompanyId and CompanyName are mandatory.';
            SET @NewId = 0;
            RETURN;
        END

        BEGIN TRANSACTION;

        -- Generate QuotationNo: SSE_YYYY_000X
        DECLARE @Year NVARCHAR(4) = CAST(YEAR(GETDATE()) AS NVARCHAR(4));
        DECLARE @Prefix NVARCHAR(10) = 'SSE_' + @Year + '_';
        DECLARE @NextNumber INT;
        DECLARE @QuotationNo NVARCHAR(50);

        SELECT @NextNumber = ISNULL(MAX(CAST(RIGHT([QuotationNo], 4) AS INT)), 0) + 1
        FROM [dbo].[RateQuotation] WITH (UPDLOCK, HOLDLOCK)
        WHERE [QuotationNo] LIKE @Prefix + '%';

        SET @QuotationNo = @Prefix + RIGHT('0000' + CAST(@NextNumber AS NVARCHAR(4)), 4);

        INSERT INTO [dbo].[RateQuotation] (
            [QuotationNo], [QuotationDate], [CompanyId], [CompanyName], [ContactPerson],
            [MobileNo], [EmailId], [Address], [StyleNo], [DesignName], [ProductType],
            [RatePerPiece], [RatePerMeter], [NoOfStitches], [ChenilleColors], [NormalEmbColors], [Quantity], [TotalAmount], [Remarks],
            [Status], [IsActive], [CreatedBy], [CreatedDate]
        )
        VALUES (
            @QuotationNo, @QuotationDate, @CompanyId, @CompanyName, @ContactPerson,
            @MobileNo, @EmailId, @Address, @StyleNo, @DesignName, @ProductType,
            @RatePerPiece, @RatePerMeter, @NoOfStitches, @ChenilleColors, @NormalEmbColors, @Quantity, @TotalAmount, @Remarks,
            @Status, 1, @CreatedBy, GETDATE()
        );

        SET @NewId = SCOPE_IDENTITY();
        SET @StatusCode = 201;
        SET @StatusMessage = 'Rate Quotation created successfully.';
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @StatusCode = 500;
        SET @StatusMessage = ERROR_MESSAGE();
        SET @NewId = 0;
    END CATCH
END
GO

-- B. USP_RateQuotation_Update
IF OBJECT_ID('USP_RateQuotation_Update', 'P') IS NOT NULL
    DROP PROCEDURE USP_RateQuotation_Update
GO
CREATE PROCEDURE [dbo].[USP_RateQuotation_Update]
    @Id BIGINT,
    @QuotationDate DATETIME,
    @CompanyId BIGINT,
    @CompanyName NVARCHAR(200),
    @ContactPerson NVARCHAR(200) = NULL,
    @MobileNo NVARCHAR(20) = NULL,
    @EmailId NVARCHAR(200) = NULL,
    @Address NVARCHAR(MAX) = NULL,
    @StyleNo NVARCHAR(100) = NULL,
    @DesignName NVARCHAR(200) = NULL,
    @ProductType NVARCHAR(100) = NULL,
    @RatePerPiece DECIMAL(18,2) = NULL,
    @RatePerMeter DECIMAL(18,2) = NULL,
    @NoOfStitches INT = NULL,
    @ChenilleColors INT = NULL,
    @NormalEmbColors INT = NULL,
    @Quantity INT,
    @TotalAmount DECIMAL(18,2),
    @Remarks NVARCHAR(MAX) = NULL,
    @Status NVARCHAR(20),
    @ModifiedBy BIGINT,
    @StatusCode INT OUTPUT,
    @StatusMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [dbo].[RateQuotation] WHERE [Id] = @Id AND [IsActive] = 1)
        BEGIN
            SET @StatusCode = 404;
            SET @StatusMessage = 'Rate Quotation not found.';
            RETURN;
        END

        IF @CompanyId <= 0 OR ISNULL(@CompanyName, '') = ''
        BEGIN
            SET @StatusCode = 400;
            SET @StatusMessage = 'CompanyId and CompanyName are mandatory.';
            RETURN;
        END

        BEGIN TRANSACTION;

        UPDATE [dbo].[RateQuotation]
        SET [QuotationDate] = @QuotationDate,
            [CompanyId] = @CompanyId,
            [CompanyName] = @CompanyName,
            [ContactPerson] = @ContactPerson,
            [MobileNo] = @MobileNo,
            [EmailId] = @EmailId,
            [Address] = @Address,
            [StyleNo] = @StyleNo,
            [DesignName] = @DesignName,
            [ProductType] = @ProductType,
            [RatePerPiece] = @RatePerPiece,
            [RatePerMeter] = @RatePerMeter,
            [NoOfStitches] = @NoOfStitches,
            [ChenilleColors] = @ChenilleColors,
            [NormalEmbColors] = @NormalEmbColors,
            [Quantity] = @Quantity,
            [TotalAmount] = @TotalAmount,
            [Remarks] = @Remarks,
            [Status] = @Status,
            [ModifiedBy] = @ModifiedBy,
            [ModifiedDate] = GETDATE()
        WHERE [Id] = @Id;

        SET @StatusCode = 200;
        SET @StatusMessage = 'Rate Quotation updated successfully.';
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @StatusCode = 500;
        SET @StatusMessage = ERROR_MESSAGE();
    END CATCH
END
GO

-- C. USP_RateQuotation_Delete
IF OBJECT_ID('USP_RateQuotation_Delete', 'P') IS NOT NULL
    DROP PROCEDURE USP_RateQuotation_Delete
GO
CREATE PROCEDURE [dbo].[USP_RateQuotation_Delete]
    @Id BIGINT,
    @ModifiedBy BIGINT,
    @StatusCode INT OUTPUT,
    @StatusMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [dbo].[RateQuotation] WHERE [Id] = @Id AND [IsActive] = 1)
        BEGIN
            SET @StatusCode = 404;
            SET @StatusMessage = 'Rate Quotation not found.';
            RETURN;
        END

        BEGIN TRANSACTION;

        UPDATE [dbo].[RateQuotation]
        SET [IsActive] = 0,
            [ModifiedBy] = @ModifiedBy,
            [ModifiedDate] = GETDATE()
        WHERE [Id] = @Id;

        SET @StatusCode = 200;
        SET @StatusMessage = 'Rate Quotation deleted successfully.';
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @StatusCode = 500;
        SET @StatusMessage = ERROR_MESSAGE();
    END CATCH
END
GO

-- D. USP_RateQuotation_GetById
IF OBJECT_ID('USP_RateQuotation_GetById', 'P') IS NOT NULL
    DROP PROCEDURE USP_RateQuotation_GetById
GO
CREATE PROCEDURE [dbo].[USP_RateQuotation_GetById]
    @Id BIGINT,
    @StatusCode INT OUTPUT,
    @StatusMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [dbo].[RateQuotation] WHERE [Id] = @Id AND [IsActive] = 1)
        BEGIN
            SET @StatusCode = 404;
            SET @StatusMessage = 'Rate Quotation not found.';
            RETURN;
        END

        SELECT * FROM [dbo].[RateQuotation] WHERE [Id] = @Id AND [IsActive] = 1;

        SET @StatusCode = 200;
        SET @StatusMessage = 'Success';
    END TRY
    BEGIN CATCH
        SET @StatusCode = 500;
        SET @StatusMessage = ERROR_MESSAGE();
    END CATCH
END
GO

-- E. USP_RateQuotation_GetAll
IF OBJECT_ID('USP_RateQuotation_GetAll', 'P') IS NOT NULL
    DROP PROCEDURE USP_RateQuotation_GetAll
GO
CREATE PROCEDURE [dbo].[USP_RateQuotation_GetAll]
    @StatusCode INT OUTPUT,
    @StatusMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT * FROM [dbo].[RateQuotation] WHERE [IsActive] = 1 ORDER BY [CreatedDate] DESC;

        SET @StatusCode = 200;
        SET @StatusMessage = 'Success';
    END TRY
    BEGIN CATCH
        SET @StatusCode = 500;
        SET @StatusMessage = ERROR_MESSAGE();
    END CATCH
END
GO

-- F. USP_RateQuotation_Search
IF OBJECT_ID('USP_RateQuotation_Search', 'P') IS NOT NULL
    DROP PROCEDURE USP_RateQuotation_Search
GO
CREATE PROCEDURE [dbo].[USP_RateQuotation_Search]
    @QuotationNo NVARCHAR(50) = NULL,
    @CompanyName NVARCHAR(200) = NULL,
    @StyleNo NVARCHAR(100) = NULL,
    @DesignName NVARCHAR(200) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @Status NVARCHAR(20) = NULL,
    @StatusCode INT OUTPUT,
    @StatusMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT * FROM [dbo].[RateQuotation]
        WHERE [IsActive] = 1
        AND (@QuotationNo IS NULL OR [QuotationNo] LIKE '%' + @QuotationNo + '%')
        AND (@CompanyName IS NULL OR [CompanyName] LIKE '%' + @CompanyName + '%')
        AND (@StyleNo IS NULL OR [StyleNo] LIKE '%' + @StyleNo + '%')
        AND (@DesignName IS NULL OR [DesignName] LIKE '%' + @DesignName + '%')
        AND (@FromDate IS NULL OR CAST([QuotationDate] AS DATE) >= CAST(@FromDate AS DATE))
        AND (@ToDate IS NULL OR CAST([QuotationDate] AS DATE) <= CAST(@ToDate AS DATE))
        AND (@Status IS NULL OR [Status] = @Status)
        ORDER BY [QuotationDate] DESC;

        SET @StatusCode = 200;
        SET @StatusMessage = 'Success';
    END TRY
    BEGIN CATCH
        SET @StatusCode = 500;
        SET @StatusMessage = ERROR_MESSAGE();
    END CATCH
END
GO

-- G. USP_RateQuotation_Pagination
IF OBJECT_ID('USP_RateQuotation_Pagination', 'P') IS NOT NULL
    DROP PROCEDURE USP_RateQuotation_Pagination
GO
CREATE PROCEDURE [dbo].[USP_RateQuotation_Pagination]
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @QuotationNo NVARCHAR(50) = NULL,
    @CompanyName NVARCHAR(200) = NULL,
    @StyleNo NVARCHAR(100) = NULL,
    @DesignName NVARCHAR(200) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @Status NVARCHAR(20) = NULL,
    @TotalRecords INT OUTPUT,
    @StatusCode INT OUTPUT,
    @StatusMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Get total records for the filter
        SELECT @TotalRecords = COUNT(*)
        FROM [dbo].[RateQuotation]
        WHERE [IsActive] = 1
        AND (@QuotationNo IS NULL OR [QuotationNo] LIKE '%' + @QuotationNo + '%')
        AND (@CompanyName IS NULL OR [CompanyName] LIKE '%' + @CompanyName + '%')
        AND (@StyleNo IS NULL OR [StyleNo] LIKE '%' + @StyleNo + '%')
        AND (@DesignName IS NULL OR [DesignName] LIKE '%' + @DesignName + '%')
        AND (@FromDate IS NULL OR CAST([QuotationDate] AS DATE) >= CAST(@FromDate AS DATE))
        AND (@ToDate IS NULL OR CAST([QuotationDate] AS DATE) <= CAST(@ToDate AS DATE))
        AND (@Status IS NULL OR [Status] = @Status);

        -- Get paginated records
        SELECT *
        FROM [dbo].[RateQuotation]
        WHERE [IsActive] = 1
        AND (@QuotationNo IS NULL OR [QuotationNo] LIKE '%' + @QuotationNo + '%')
        AND (@CompanyName IS NULL OR [CompanyName] LIKE '%' + @CompanyName + '%')
        AND (@StyleNo IS NULL OR [StyleNo] LIKE '%' + @StyleNo + '%')
        AND (@DesignName IS NULL OR [DesignName] LIKE '%' + @DesignName + '%')
        AND (@FromDate IS NULL OR CAST([QuotationDate] AS DATE) >= CAST(@FromDate AS DATE))
        AND (@ToDate IS NULL OR CAST([QuotationDate] AS DATE) <= CAST(@ToDate AS DATE))
        AND (@Status IS NULL OR [Status] = @Status)
        ORDER BY [QuotationDate] DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SET @StatusCode = 200;
        SET @StatusMessage = 'Success';
    END TRY
    BEGIN CATCH
        SET @StatusCode = 500;
        SET @StatusMessage = ERROR_MESSAGE();
        SET @TotalRecords = 0;
    END CATCH
END
GO
