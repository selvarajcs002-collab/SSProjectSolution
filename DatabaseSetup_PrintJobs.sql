USE [SSManagementTEST] -- Update to your actual DB name if different
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrintJobs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PrintJobs](
        [JobId] [nvarchar](50) NOT NULL,
        [DocumentType] [nvarchar](50) NOT NULL,
        [DocumentNumber] [nvarchar](50) NOT NULL,
        [PdfPath] [nvarchar](max) NOT NULL,
        [PrinterName] [nvarchar](255) NULL,
        [Copies] [int] NOT NULL DEFAULT(1),
        [PaperSize] [nvarchar](50) NULL,
        [Orientation] [nvarchar](50) NULL,
        [Status] [nvarchar](50) NOT NULL, -- Queued, Sent, Printed, Failed
        [RetryCount] [int] NOT NULL DEFAULT(0),
        [UserId] [nvarchar](50) NOT NULL,
        [CompanyId] [int] NULL,
        [CreatedDate] [datetime] NOT NULL DEFAULT(GETDATE()),
        [CompletedDate] [datetime] NULL,
        [FailureReason] [nvarchar](max) NULL,
        [Downloaded] [bit] NOT NULL DEFAULT(0),
        [Printed] [bit] NOT NULL DEFAULT(0),
     CONSTRAINT [PK_PrintJobs] PRIMARY KEY CLUSTERED 
    (
        [JobId] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
