-- Add Additional Details columns to Outward table
ALTER TABLE Outward ADD 
    DeliveryTo NVARCHAR(150) NULL,
    PoNo NVARCHAR(100) NULL,
    Weight NVARCHAR(100) NULL,
    NoOfBundles NVARCHAR(100) NULL;
GO
