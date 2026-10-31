SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND type in (N'U'))
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'CompanyName')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [CompanyName] NVARCHAR(200) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'ContactPerson')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [ContactPerson] NVARCHAR(200) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'MobileNo')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [MobileNo] NVARCHAR(20) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'EmailId')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [EmailId] NVARCHAR(200) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'Address')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [Address] NVARCHAR(MAX) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'StyleNo')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [StyleNo] NVARCHAR(100) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'DesignName')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [DesignName] NVARCHAR(200) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'ProductType')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [ProductType] NVARCHAR(100) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'RatePerPiece')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [RatePerPiece] NVARCHAR(200) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'RatePerMeter')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [RatePerMeter] NVARCHAR(200) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'NoOfStitches')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [NoOfStitches] NVARCHAR(100) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'NumberOfTrimmings')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [NumberOfTrimmings] NVARCHAR(200) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'CustomField')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [CustomField] NVARCHAR(500) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'ChenilleColors')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [ChenilleColors] NVARCHAR(100) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'NormalEmbColors')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [NormalEmbColors] NVARCHAR(100) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'TotalAmount')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [TotalAmount] NVARCHAR(200) NULL;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RateQuotation]') AND name = 'Remarks')
        ALTER TABLE [dbo].[RateQuotation] ALTER COLUMN [Remarks] NVARCHAR(MAX) NULL;
END
GO
