-- Script to create WhatsAppMessageLog table for duplicate protection and audit logging

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WhatsAppMessageLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WhatsAppMessageLog] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [OperationType] VARCHAR(50) NOT NULL, -- 'INWARD' or 'OUTWARD'
        [ReferenceId] INT NOT NULL,           -- InwardId or OutwardId
        [GroupId] VARCHAR(100) NOT NULL,
        [MessageText] NVARCHAR(MAX) NOT NULL,
        [ProviderMessageId] VARCHAR(100) NULL,
        [Status] VARCHAR(20) NOT NULL,        -- 'PENDING', 'SENT', 'FAILED'
        [RetryCount] INT NOT NULL DEFAULT 0,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [SentDate] DATETIME NULL
    );

    -- Unique constraint for duplicate protection
    ALTER TABLE [dbo].[WhatsAppMessageLog] 
    ADD CONSTRAINT [UQ_WhatsAppMessageLog_Op_Ref] UNIQUE ([OperationType], [ReferenceId]);
    
    PRINT 'Table WhatsAppMessageLog created successfully.';
END
ELSE
BEGIN
    PRINT 'Table WhatsAppMessageLog already exists.';
END
GO
