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
ALTER PROCEDURE [dbo].[SP_SAVE_INWARD_METER]
    @InwardId INT,
    @CompanyId INT,
    @Colour NVARCHAR(100),
    @DesignName NVARCHAR(150),
    @StyleNo NVARCHAR(100),
    @InwardDcNo NVARCHAR(100),
    @PoNo NVARCHAR(100) = NULL,
    @EntryType CHAR(1) = 'M',
    @CreatedBy INT,
    @MeterDetails MeterDetailType READONLY,
    @InwardDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Trim strings
        SET @Colour = LTRIM(RTRIM(@Colour));
        SET @DesignName = LTRIM(RTRIM(@DesignName));
        SET @StyleNo = LTRIM(RTRIM(@StyleNo));
        SET @InwardDcNo = ISNULL(LTRIM(RTRIM(@InwardDcNo)), '');
        IF @PoNo IS NOT NULL SET @PoNo = LTRIM(RTRIM(@PoNo));

        DECLARE @CurrentInwardId INT = @InwardId;

        -- Save inward master
        IF @CurrentInwardId = 0
        BEGIN
            INSERT INTO Inward (
                CompanyId, 
                Colour, 
                DesignName, 
                StyleNo, 
                InwardDcNo,
                PoNo,
                InwardEntryType,
                CreatedBy, 
                CreatedDate,
                InwardDate
            )
            VALUES (
                @CompanyId, 
                @Colour, 
                @DesignName, 
                @StyleNo, 
                @InwardDcNo,
                @PoNo,
                @EntryType,
                @CreatedBy, 
                GETDATE(),
                @InwardDate
            );
            SET @CurrentInwardId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE Inward
            SET Colour = @Colour,
                DesignName = @DesignName,
                StyleNo = @StyleNo,
                InwardDcNo = @InwardDcNo,
                PoNo = @PoNo,
                UpdatedDate = GETDATE(),
                InwardDate = ISNULL(@InwardDate, InwardDate)
            WHERE InwardId = @CurrentInwardId AND CompanyId = @CompanyId;
        END

        -- Delete existing meter details
        DELETE FROM INWARD_METER_DETAIL WHERE IMD_INWARD_ID = @CurrentInwardId;

        -- Insert new meter details
        IF EXISTS (SELECT 1 FROM @MeterDetails)
        BEGIN
            INSERT INTO INWARD_METER_DETAIL (
                IMD_INWARD_ID,
                IMD_COMPANY_ID,
                IMD_METER_VALUE,
                IMD_BITS_COUNT,
                IMD_TOTAL_METER,
                IMD_CREATED_BY,
                IMD_CREATED_DATE
            )
            SELECT 
                @CurrentInwardId,
                @CompanyId,
                MeterValue,
                BitsCount,
                (MeterValue * BitsCount),
                @CreatedBy,
                GETDATE()
            FROM @MeterDetails
            WHERE MeterValue > 0 AND BitsCount > 0;
        END

        COMMIT TRANSACTION;
        SELECT @CurrentInwardId AS InwardId, 'Meter Inward Saved Successfully' AS [Message];
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrorMessage, 16, 1);
    END CATCH
END
GO
ALTER PROCEDURE [dbo].[usp_GetLastTransactions]
(
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @CompanyId INT = NULL,
    @StyleNo NVARCHAR(50) = NULL,
    @DesignName NVARCHAR(100) = NULL,
    @Colour NVARCHAR(100) = NULL,
    @TopCount INT = 50
)
AS
BEGIN
    SET NOCOUNT ON;

    WITH AllTransactions AS (
        -- Inward
        SELECT 
            I.InwardId AS Id,
            COALESCE(I.InwardDate, I.CreatedDate) AS [Date],
            I.CreatedDate AS ActualCreatedDate,
            'INWARD' AS [Type],
            I.InwardDcNo AS DcNo,
            C.CompanyName,
            I.StyleNo,
            I.DesignName,
            I.Colour AS Color,
            (SELECT ISNULL(SUM(ISC.[Count]), 0) FROM InwardSizeCount ISC WHERE ISC.InwardId = I.InwardId) AS InwardQty,
            NULL AS OutwardQty
        FROM Inward I
        LEFT JOIN CompanyDetails C ON I.CompanyId = C.CompanyId
        WHERE (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR I.Colour LIKE '%' + @Colour + '%')
          AND (@FromDate IS NULL OR CAST(I.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(I.CreatedDate AS DATE) <= @ToDate)
          AND I.Status <> 'Deleted'

        UNION ALL

        -- Outward
        SELECT 
            O.OutwardId AS Id,
            O.CreatedDate AS [Date],
            O.CreatedDate AS ActualCreatedDate,
            'OUTWARD' AS [Type],
            O.OutwardDcNo AS DcNo,
            C.CompanyName,
            O.StyleNo,
            O.DesignName,
            CASE 
                WHEN O.Colour = 'MULTI' THEN 
                    COALESCE(STUFF((SELECT ', ' + Colour FROM OutwardColour WHERE OutwardId = O.OutwardId FOR XML PATH('')), 1, 2, ''), O.Colour)
                ELSE O.Colour 
            END AS Color,
            NULL AS InwardQty,
            (SELECT ISNULL(SUM(OSC.[Count]), 0) FROM OutwardSizeCount OSC WHERE OSC.OutwardId = O.OutwardId) AS OutwardQty
        FROM Outward O
        LEFT JOIN CompanyDetails C ON O.CompanyId = C.CompanyId
        WHERE (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
          AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
          AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
          AND (@Colour IS NULL OR O.Colour LIKE '%' + @Colour + '%' OR EXISTS (SELECT 1 FROM OutwardColour WHERE OutwardId = O.OutwardId AND Colour LIKE '%' + @Colour + '%'))
          AND (@FromDate IS NULL OR CAST(O.CreatedDate AS DATE) >= @FromDate)
          AND (@ToDate IS NULL OR CAST(O.CreatedDate AS DATE) <= @ToDate)
          AND O.Status <> 'Deleted'
    )
    SELECT TOP (@TopCount) Id, [Date], [Type], DcNo, CompanyName, StyleNo, DesignName, Color, InwardQty, OutwardQty
    FROM AllTransactions
    ORDER BY ActualCreatedDate DESC;
END
GO
ALTER PROCEDURE [dbo].[usp_GetInwardOutwardDetails_Filter]
                                                                                                                                                                                                
(
                                                                                                                                                                                                                                                            
    @Mode NVARCHAR(10) = NULL,        -- 'INWARD' / 'OUTWARD' / NULL (both)
                                                                                                                                                                                  
    @FromDate DATE = NULL,
                                                                                                                                                                                                                                   
    @ToDate DATE = NULL,
                                                                                                                                                                                                                                     
    @CompanyId INT = NULL,
                                                                                                                                                                                                                                   
    @StyleNo NVARCHAR(50) = NULL,
                                                                                                                                                                                                                            
    @DesignName NVARCHAR(100) = NULL
                                                                                                                                                                                                                         
)
                                                                                                                                                                                                                                                            
AS
                                                                                                                                                                                                                                                           
BEGIN
                                                                                                                                                                                                                                                        
    SET NOCOUNT ON;
                                                                                                                                                                                                                                          

                                                                                                                                                                                                                                                             
    --------------------------------------------------
                                                                                                                                                                                                       
    -- INWARD DATA
                                                                                                                                                                                                                                           
    --------------------------------------------------
                                                                                                                                                                                                       
    IF (@Mode = 'INWARD' OR @Mode IS NULL)
                                                                                                                                                                                                                   
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
                                                                                                                                                                                                                                     
            COALESCE(I.InwardDate, I.CreatedDate) AS CreatedDate,
                                                                                                                                                                                                                                   
            I.UpdatedDate,
                                                                                                                                                                                                                                   
            I.InwardDcNo,
                                                                                                                                                                                                                                    
            I.Status,
                                                                                                                                                                                                                                        

                                                                                                                                                                                                                                                             
            ISC.Id AS SizeCountId,
                                                                                                                                                                                                                           
            ISC.Size,
                                                                                                                                                                                                                                        
            ISC.Count
                                                                                                                                                                                                                                        

                                                                                                                                                                                                                                                             
        FROM SSManagementDEV.dbo.Inward I
                                                                                                                                                                                                                    
        LEFT JOIN SSManagementDEV.dbo.InwardSizeCount ISC
                                                                                                                                                                                                    
            ON I.InwardId = ISC.InwardId
                                                                                                                                                                                                                     
        LEFT JOIN SSManagementDEV.dbo.CompanyDetails cmp
                                                                                                                                                                                                     
            ON I.CompanyId = cmp.CompanyId
                                                                                                                                                                                                                   

                                                                                                                                                                                                                                                             
        WHERE
                                                                                                                                                                                                                                                
            (@FromDate IS NULL OR CAST(I.CreatedDate AS DATE) >= @FromDate)
                                                                                                                                                                                  
            AND (@ToDate IS NULL OR CAST(I.CreatedDate AS DATE) <= @ToDate)
                                                                                                                                                                                  
            AND (@CompanyId IS NULL OR I.CompanyId = @CompanyId)
                                                                                                                                                                                             
            AND (@StyleNo IS NULL OR I.StyleNo LIKE '%' + @StyleNo + '%')
                                                                                                                                                                                    
            AND (@DesignName IS NULL OR I.DesignName LIKE '%' + @DesignName + '%')
                                                                                                                                                                           
    END
                                                                                                                                                                                                                                                      

                                                                                                                                                                                                                                                             
    --------------------------------------------------
                                                                                                                                                                                                       
    -- OUTWARD DATA
                                                                                                                                                                                                                                          
    --------------------------------------------------
                                                                                                                                                                                                       
    IF (@Mode = 'OUTWARD' OR @Mode IS NULL)
                                                                                                                                                                                                                  
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
                                                                                                                                                                                                                                        

                                                                                                                                                                                                                                                             
            OSC.Id AS SizeCountId,
                                                                                                                                                                                                                           
            OSC.Size,
                                                                                                                                                                                                                                        
            OSC.Count
                                                                                                                                                                                                                                        

                                                                                                                                                                                                                                                             
        FROM SSManagementDEV.dbo.Outward O
                                                                                                                                                                                                                   
        LEFT JOIN SSManagementDEV.dbo.OutwardSizeCount OSC
                                                                                                                                                                                                   
            ON O.OutwardId = OSC.OutwardId
                                                                                                                                                                                                                   
        LEFT JOIN SSManagementDEV.dbo.CompanyDetails cmp
                                                                                                                                                                                                     
            ON O.CompanyId = cmp.CompanyId
                                                                                                                                                                                                                   

                                                                                                                                                                                                                                                             
        WHERE
                                                                                                                                                                                                                                                
            (@FromDate IS NULL OR CAST(O.CreatedDate AS DATE) >= @FromDate)
                                                                                                                                                                                  
            AND (@ToDate IS NULL OR CAST(O.CreatedDate AS DATE) <= @ToDate)
                                                                                                                                                                                  
            AND (@CompanyId IS NULL OR O.CompanyId = @CompanyId)
                                                                                                                                                                                             
            AND (@StyleNo IS NULL OR O.StyleNo LIKE '%' + @StyleNo + '%')
                                                                                                                                                                                    
            AND (@DesignName IS NULL OR O.DesignName LIKE '%' + @DesignName + '%')
                                                                                                                                                                           
    END
                                                                                                                                                                                                                                                      

                                                                                                                                                                                                                                                             
END
                                                                                                                                                                                                                                                          
ALTER PROCEDURE [dbo].[usp_GetInwardOutwardDetails]
                                                                                                                                                                                                       
(
                                                                                                                                                                                                                                                            
    @Mode NVARCHAR(10) = NULL,     -- 'INWARD' / 'OUTWARD' / NULL
                                                                                                                                                                                            
    @PageNumber INT = 1,
                                                                                                                                                                                                                                     
    @PageSize INT = 10
                                                                                                                                                                                                                                       
)
                                                                                                                                                                                                                                                            
AS
                                                                                                                                                                                                                                                           
BEGIN
                                                                                                                                                                                                                                                        
    SET NOCOUNT ON;
                                                                                                                                                                                                                                          

                                                                                                                                                                                                                                                             
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
                                                                                                                                                                                                     

                                                                                                                                                                                                                                                             
    -----------------------------------------
                                                                                                                                                                                                                
    -- INWARD DATA (LATEST + PAGINATION)
                                                                                                                                                                                                                     
    -----------------------------------------
                                                                                                                                                                                                                
    IF (@Mode = 'INWARD' OR @Mode IS NULL)
                                                                                                                                                                                                                   
    BEGIN
                                                                                                                                                                                                                                                    
        ;WITH InwardCTE AS
                                                                                                                                                                                                                                   
        (
                                                                                                                                                                                                                                                    
            SELECT 
                                                                                                                                                                                                                                          
                I.InwardId,
                                                                                                                                                                                                                                  
                UPPER(cmp.CompanyName) AS CompanyName,
                                                                                                                                                                                                       
                I.CompanyId,
                                                                                                                                                                                                                                 
                I.Colour,
                                                                                                                                                                                                                                    
                I.DesignName,
                                                                                                                                                                                                                                
                I.StyleNo,
                                                                                                                                                                                                                                   
                I.UploadURL,
                                                                                                                                                                                                                                 
                I.CreatedBy,
                                                                                                                                                                                                                                 
                COALESCE(I.InwardDate, I.CreatedDate) AS CreatedDate,
                                                                                                                                                                                                                               
                I.UpdatedDate,
                                                                                                                                                                                                                               
                I.InwardDcNo,
                                                                                                                                                                                                                                
                I.Status,
                                                                                                                                                                                                                                    
                ISC.Id AS SizeCountId,
                                                                                                                                                                                                                       
                ISC.Size,
                                                                                                                                                                                                                                    
                ISC.Count,
                                                                                                                                                                                                                                   

                                                                                                                                                                                                                                                             
                ROW_NUMBER() OVER (PARTITION BY I.InwardId ORDER BY I.CreatedDate DESC) AS rn,
                                                                                                                                                               
                DENSE_RANK() OVER (ORDER BY I.CreatedDate DESC) AS RowNum
                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
            FROM SSManagementDEV.dbo.Inward I
                                                                                                                                                                                                                
            LEFT JOIN SSManagementDEV.dbo.InwardSizeCount ISC
                                                                                                                                                                                                
                ON I.InwardId = ISC.InwardId
                                                                                                                                                                                                                 
            LEFT JOIN SSManagementDEV.dbo.CompanyDetails cmp
                                                                                                                                                                                                 
                ON I.CompanyId = cmp.CompanyId
                                                                                                                                                                                                               
        )
                                                                                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
        SELECT *
                                                                                                                                                                                                                                             
        FROM InwardCTE
                                                                                                                                                                                                                                       
        WHERE RowNum > @Offset AND RowNum <= (@Offset + @PageSize)
                                                                                                                                                                                           
        ORDER BY CreatedDate DESC;
                                                                                                                                                                                                                           
    END
                                                                                                                                                                                                                                                      

                                                                                                                                                                                                                                                             
    -----------------------------------------
                                                                                                                                                                                                                
    -- OUTWARD DATA (LATEST + PAGINATION)
                                                                                                                                                                                                                    
    -----------------------------------------
                                                                                                                                                                                                                
    IF (@Mode = 'OUTWARD' OR @Mode IS NULL)
                                                                                                                                                                                                                  
    BEGIN
                                                                                                                                                                                                                                                    
        ;WITH OutwardCTE AS
                                                                                                                                                                                                                                  
        (
                                                                                                                                                                                                                                                    
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
                                                                                                                                                                                                                                    
                OSC.Id AS SizeCountId,
                                                                                                                                                                                                                       
                OSC.Size,
                                                                                                                                                                                                                                    
                OSC.Count,
                                                                                                                                                                                                                                   

                                                                                                                                                                                                                                                             
                ROW_NUMBER() OVER (PARTITION BY O.OutwardId ORDER BY O.CreatedDate DESC) AS rn,
                                                                                                                                                              
                DENSE_RANK() OVER (ORDER BY O.CreatedDate DESC) AS RowNum
                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
            FROM SSManagementDEV.dbo.Outward O
                                                                                                                                                                                                               
            LEFT JOIN SSManagementDEV.dbo.OutwardSizeCount OSC
                                                                                                                                                                                               
                ON O.OutwardId = OSC.OutwardId
                                                                                                                                                                                                               
            LEFT JOIN SSManagementDEV.dbo.CompanyDetails cmp
                                                                                                                                                                                                 
                ON O.CompanyId = cmp.CompanyId
                                                                                                                                                                                                               
        )
                                                                                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
        SELECT *
                                                                                                                                                                                                                                             
        FROM OutwardCTE
                                                                                                                                                                                                                                      
        WHERE RowNum > @Offset AND RowNum <= (@Offset + @PageSize)
                                                                                                                                                                                           
        ORDER BY CreatedDate DESC;
                                                                                                                                                                                                                           
    END
                                                                                                                                                                                                                                                      

                                                                                                                                                                                                                                                             
END
                                                                                                                                                                                                                                                          
