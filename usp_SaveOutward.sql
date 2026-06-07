Text                                                                                                                                                                                                                                                           
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                                                                                                                                                                                                                                                             
CREATE PROCEDURE [dbo].[usp_SaveOutward]
                                                                                                                                                                                                                     
(
                                                                                                                                                                                                                                                            
    @Mode NVARCHAR(10), -- 'INSERT' / 'UPDATE'
                                                                                                                                                                                                               

                                                                                                                                                                                                                                                             
    @OutwardId INT = NULL OUTPUT,
                                                                                                                                                                                                                            
    @CompanyId INT,
                                                                                                                                                                                                                                          
    @Colour NVARCHAR(50),
                                                                                                                                                                                                                                    
    @DesignName NVARCHAR(100),
                                                                                                                                                                                                                               
    @StyleNo NVARCHAR(50),
                                                                                                                                                                                                                                   
    @UploadURL NVARCHAR(255),
                                                                                                                                                                                                                                
    @CreatedBy NVARCHAR(100),
                                                                                                                                                                                                                                
    @OutwardDcNo NVARCHAR(50) = NULL OUTPUT,
                                                                                                                                                                                                                 
    @Status NVARCHAR(50),
                                                                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
    @SizeData NVARCHAR(MAX) -- JSON
                                                                                                                                                                                                                          
)
                                                                                                                                                                                                                                                            
AS
                                                                                                                                                                                                                                                           
BEGIN
                                                                                                                                                                                                                                                        
    SET NOCOUNT ON;
                                                                                                                                                                                                                                          

                                                                                                                                                                                                                                                             
    BEGIN TRY
                                                                                                                                                                                                                                                

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- VALIDATE MODE
                                                                                                                                                                                                                                     
        -----------------------------------------
                                                                                                                                                                                                            
        IF (@Mode NOT IN ('INSERT', 'UPDATE'))
                                                                                                                                                                                                               
        BEGIN
                                                                                                                                                                                                                                                
            SELECT 0 AS Success, 'Invalid Mode' AS Message, NULL AS OutwardId, NULL AS OutwardDcNo;
                                                                                                                                                          
            RETURN;
                                                                                                                                                                                                                                          
        END
                                                                                                                                                                                                                                                  

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- EXTRACT SIZE DATA
                                                                                                                                                                                                                                 
        -----------------------------------------
                                                                                                                                                                                                            
        DECLARE @InputSizes TABLE
                                                                                                                                                                                                                            
        (
                                                                                                                                                                                                                                                    
            StyleNo NVARCHAR(50),
                                                                                                                                                                                                                            
            DesignName NVARCHAR(100),
                                                                                                                                                                                                                        
            Colour NVARCHAR(50),
                                                                                                                                                                                                                             
            Size NVARCHAR(20),
                                                                                                                                                                                                                               
            Count INT
                                                                                                                                                                                                                                        
        );
                                                                                                                                                                                                                                                   

                                                                                                                                                                                                                                                             
        INSERT INTO @InputSizes
                                                                                                                                                                                                                              
        SELECT 
                                                                                                                                                                                                                                              
            @StyleNo,
                                                                                                                                                                                                                                        
            @DesignName,
                                                                                                                                                                                                                                     
            @Colour,
                                                                                                                                                                                                                                         
            Size,
                                                                                                                                                                                                                                            
            Count
                                                                                                                                                                                                                                            
        FROM OPENJSON(@SizeData, '$.sizes')
                                                                                                                                                                                                                  
        WITH
                                                                                                                                                                                                                                                 
        (
                                                                                                                                                                                                                                                    
            Size NVARCHAR(20) '$.size',
                                                                                                                                                                                                                      
            Count INT '$.count'
                                                                                                                                                                                                                              
        );
                                                                                                                                                                                                                                                   

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- INWARD STOCK
                                                                                                                                                                                                                                      
        -----------------------------------------
                                                                                                                                                                                                            
        DECLARE @Inward TABLE
                                                                                                                                                                                                                                
        (
                                                                                                                                                                                                                                                    
            StyleNo NVARCHAR(50),
                                                                                                                                                                                                                            
            DesignName NVARCHAR(100),
                                                                                                                                                                                                                        
            Colour NVARCHAR(50),
                                                                                                                                                                                                                             
            Size NVARCHAR(20),
                                                                                                                                                                                                                               
            TotalInward INT
                                                                                                                                                                                                                                  
        );
                                                                                                                                                                                                                                                   

                                                                                                                                                                                                                                                             
        INSERT INTO @Inward
                                                                                                                                                                                                                                  
        SELECT 
                                                                                                                                                                                                                                              
            StyleNo, DesignName, Colour, Size,
                                                                                                                                                                                                               
            SUM([Count])
                                                                                                                                                                                                                                     
        FROM SSManagement.dbo.InwardSizeCount
                                                                                                                                                                                                                
        GROUP BY StyleNo, DesignName, Colour, Size;
                                                                                                                                                                                                          

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- OUTWARD USED (IMPORTANT FOR UPDATE)
                                                                                                                                                                                                               
        -----------------------------------------
                                                                                                                                                                                                            
        DECLARE @OutwardUsed TABLE
                                                                                                                                                                                                                           
        (
                                                                                                                                                                                                                                                    
            StyleNo NVARCHAR(50),
                                                                                                                                                                                                                            
            DesignName NVARCHAR(100),
                                                                                                                                                                                                                        
            Colour NVARCHAR(50),
                                                                                                                                                                                                                             
            Size NVARCHAR(20),
                                                                                                                                                                                                                               
            TotalOutward INT
                                                                                                                                                                                                                                 
        );
                                                                                                                                                                                                                                                   

                                                                                                                                                                                                                                                             
        INSERT INTO @OutwardUsed
                                                                                                                                                                                                                             
        SELECT 
                                                                                                                                                                                                                                              
            StyleNo, DesignName, Colour, Size,
                                                                                                                                                                                                               
            SUM([Count])
                                                                                                                                                                                                                                     
        FROM SSManagement.dbo.OutwardSizeCount
                                                                                                                                                                                                               
        WHERE (@Mode = 'INSERT' OR OutwardId <> @OutwardId)
                                                                                                                                                                                                  
        GROUP BY StyleNo, DesignName, Colour, Size;
                                                                                                                                                                                                          

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- AVAILABLE STOCK
                                                                                                                                                                                                                                   
        -----------------------------------------
                                                                                                                                                                                                            
        DECLARE @Available TABLE
                                                                                                                                                                                                                             
        (
                                                                                                                                                                                                                                                    
            StyleNo NVARCHAR(50),
                                                                                                                                                                                                                            
            DesignName NVARCHAR(100),
                                                                                                                                                                                                                        
            Colour NVARCHAR(50),
                                                                                                                                                                                                                             
            Size NVARCHAR(20),
                                                                                                                                                                                                                               
            AvailableCount INT
                                                                                                                                                                                                                               
        );
                                                                                                                                                                                                                                                   

                                                                                                                                                                                                                                                             
        INSERT INTO @Available
                                                                                                                                                                                                                               
        SELECT 
                                                                                                                                                                                                                                              
            i.StyleNo,
                                                                                                                                                                                                                                       
            i.DesignName,
                                                                                                                                                                                                                                    
            i.Colour,
                                                                                                                                                                                                                                        
            i.Size,
                                                                                                                                                                                                                                          
            ISNULL(i.TotalInward,0) - ISNULL(o.TotalOutward,0)
                                                                                                                                                                                               
        FROM @Inward i
                                                                                                                                                                                                                                       
        LEFT JOIN @OutwardUsed o
                                                                                                                                                                                                                             
            ON i.StyleNo=o.StyleNo
                                                                                                                                                                                                                           
            AND i.DesignName=o.DesignName
                                                                                                                                                                                                                    
            AND i.Colour=o.Colour
                                                                                                                                                                                                                            
            AND i.Size=o.Size;
                                                                                                                                                                                                                               

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- SIZE LEVEL VALIDATION
                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        DECLARE @ErrorMsg NVARCHAR(MAX);
                                                                                                                                                                                                                     

                                                                                                                                                                                                                                                             
        SELECT @ErrorMsg = STRING_AGG(
                                                                                                                                                                                                                       
            'Size ' + i.Size +
                                                                                                                                                                                                                               
            ' Available: ' + CAST(ISNULL(a.AvailableCount,0) AS VARCHAR) +
                                                                                                                                                                                   
            ' Given: ' + CAST(i.Count AS VARCHAR),
                                                                                                                                                                                                           
            ' | '
                                                                                                                                                                                                                                            
        )
                                                                                                                                                                                                                                                    
        FROM @InputSizes i
                                                                                                                                                                                                                                   
        LEFT JOIN @Available a
                                                                                                                                                                                                                               
            ON i.StyleNo=a.StyleNo
                                                                                                                                                                                                                           
     AND i.DesignName=a.DesignName
                                                                                                                                                                                                                           
            AND i.Colour=a.Colour
                                                                                                                                                                                                                            
            AND i.Size=a.Size
                                                                                                                                                                                                                                
        WHERE i.Count > ISNULL(a.AvailableCount,0);
                                                                                                                                                                                                          

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- TOTAL VALIDATION
                                                                                                                                                                                                                                  
        -----------------------------------------
                                                                                                                                                                                                            
        DECLARE @TotalInput INT, @TotalAvailable INT;
                                                                                                                                                                                                        

                                                                                                                                                                                                                                                             
        SELECT @TotalInput = SUM(Count) FROM @InputSizes;
                                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
        SELECT @TotalAvailable = SUM(AvailableCount)
                                                                                                                                                                                                         
        FROM @Available
                                                                                                                                                                                                                                      
        WHERE StyleNo=@StyleNo AND DesignName=@DesignName AND Colour=@Colour;
                                                                                                                                                                                

                                                                                                                                                                                                                                                             
        IF (@TotalInput > ISNULL(@TotalAvailable,0))
                                                                                                                                                                                                         
        BEGIN
                                                                                                                                                                                                                                                
            SET @ErrorMsg = ISNULL(@ErrorMsg + ' | ', '') +
                                                                                                                                                                                                  
                'Total exceeds. Available: ' +
                                                                                                                                                                                                               
                CAST(ISNULL(@TotalAvailable,0) AS VARCHAR) +
                                                                                                                                                                                                 
                ' Given: ' + CAST(@TotalInput AS VARCHAR);
                                                                                                                                                                                                   
        END
                                                                                                                                                                                                                                                  

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- FAIL VALIDATION
                                                                                                                                                                                                                                   
        -----------------------------------------
                                                                                                                                                                                                            
        IF (@ErrorMsg IS NOT NULL)
                                                                                                                                                                                                                           
        BEGIN
                                                                                                                                                                                                                                                
            SELECT 0 AS Success, @ErrorMsg AS Message, NULL AS OutwardId, NULL AS OutwardDcNo;
                                                                                                                                                               
            RETURN;
                                                                                                                                                                                                                                          
        END
                                                                                                                                                                                                                                                  

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- INSERT MODE
                                                                                                                                                                                                                                       
        -----------------------------------------
                                                                                                                                                                                                            
        IF (@Mode = 'INSERT')
                                                                                                                                                                                                                                
        BEGIN
                                                                                                                                                                                                                                                
            DECLARE @CompanyPrefix NVARCHAR(10), @MaxNo INT;
                                                                                                                                                                                                 

                                                                                                                                                                                                                                                             
            SELECT @CompanyPrefix = UPPER(LEFT(CompanyName,3))
                                                                                                                                                                                               
            FROM SSManagement.dbo.CompanyDetails
                                                                                                                                                                                                             
            WHERE CompanyId = @CompanyId;
                                                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
            IF (@CompanyPrefix IS NULL) SET @CompanyPrefix = 'COM';
                                                                                                                                                                                          

                                                                                                                                                                                                                                                             
            SELECT @MaxNo = MAX(
                                                                                                                                                                                                                             
                TRY_CAST(SUBSTRING(OutwardDcNo, CHARINDEX('_', OutwardDcNo)+1, LEN(OutwardDcNo)) AS INT)
                                                                                                                                                     
            )
                                                                                                                                                                                                                                                
            FROM SSManagement.dbo.Outward
                                                                                                                                                                                                                    
            WHERE CompanyId = @CompanyId;
                                                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
            IF (@MaxNo IS NULL) SET @MaxNo = 0;
                                                                                                                                                                                                              

                                                                                                                                                                                                                                                             
            SET @OutwardDcNo = @CompanyPrefix + '_' + RIGHT('000' + CAST(@MaxNo+1 AS VARCHAR), 3);
                                                                                                                                                           

                                                                                                                                                                                                                                                             
            INSERT INTO SSManagement.dbo.Outward
                                                                                                                                                                                                             
            (CompanyId, Colour, DesignName, StyleNo, UploadURL, CreatedBy, CreatedDate, OutwardDcNo, Status)
                                                                                                                                                 
            VALUES
                                                                                                                                                                                                                                           
            (@CompanyId, @Colour, @DesignName, @StyleNo, @UploadURL, @CreatedBy, GETDATE(), @OutwardDcNo, @Status);
                                                                                                                                          

                                                                                                                                                                                                                                                             
            SET @OutwardId = SCOPE_IDENTITY();
                                                                                                                                                                                                               

                                                                                                                                                                                                                                                             
            INSERT INTO SSManagement.dbo.OutwardSizeCount
                                                                                                                                                                                                    
            (OutwardId, StyleNo, DesignName, Colour, Size, Count)
                                                                                                                                                                                            
            SELECT OutwardId=@OutwardId, StyleNo, DesignName, Colour, Size, Count
                                                                                                                                                                            
            FROM @InputSizes;
                                                                                                                                                                                                                                

                                                                                                                                                                                                                                                             
            SELECT 1 AS Success, 'Outward inserted successfully' AS Message, @OutwardId, @OutwardDcNo;
                                                                                                                                                       
        END
                                                                                                                                                                                                                                                  

                                                                                                                                                                                                                                                             
        -----------------------------------------
                                                                                                                                                                                                            
        -- UPDATE MODE
                                                                                                                                                                                                                                       
        -----------------------------------------
                                                                                                                                                                                                            
        ELSE
                                                                                                                                                                                                                                                 
        BEGIN
                                                                                                                                                                                                                                                
            IF NOT EXISTS (SELECT 1 FROM SSManagement.dbo.Outward WHERE OutwardId = @OutwardId)
                                                                                                                                                              
            BEGIN
                                                                                                                                                                                                                                            
                SELECT 0 AS Success, 'Outward not found' AS Message, NULL, NULL;
                                                                                                                                                                             
                RETURN;
                                                                                                                                                                                                                                      
            END
                                                                                                                                                                                                                                              

                                                                                                                                                                                                                                                             
            SELECT @OutwardDcNo = OutwardDcNo
                                                                                                                                                                                                                
            FROM SSManagement.dbo.Outward
                                                                                                                                                                                                                    
            WHERE OutwardId = @OutwardId;
                                                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
            UPDATE SSManagement.dbo.Outward
                                                                                                                                                                                                                  
            SET 
                                                                                                                                                                                                                                             
                CompanyId   = @CompanyId,
                                                                                                                                                                                                                    
                Colour      = @Colour,
                                                                                                                                                                                                                       
                DesignName  = @DesignName,
                                                                                                                                                                                                                   
                StyleNo     = @StyleNo,
                                                                                                                                                                                                                      
                UploadURL   = @UploadURL,
                                                                                                                                                                                                                    
                Status      = @Status,
                                                                                                                                                                                                                       
                UpdatedDate = GETDATE()
                                                                                                                                                                                                                      
            WHERE OutwardId = @OutwardId;
                                                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
            DELETE FROM SSManagement.dbo.OutwardSizeCount
                                                                                                                                                                                                    
            WHERE OutwardId = @OutwardId;
                                                                                                                                                                                                                    

                                                                                                                                                                                                                                                             
            INSERT INTO SSManagement.dbo.OutwardSizeCount
                                                                                                                                                                                                    
            (OutwardId, StyleNo, DesignName, Colour, Size, Count)
                                                                                                                                                                                            
            SELECT @OutwardId, StyleNo, DesignName, Colour, Size, Count
            FROM @InputSizes;
                                                                                                                                                       

                                                                                                                                                                                                                                                             
            SELECT 1 AS Success, 'Outward updated successfully' AS Message, @OutwardId, @OutwardDcNo;
                                                                                                                                                        
        END
                                                                                                                                                                                                                                                  

                                                                                                                                                                                                                                                             
    END TRY
                                                                                                                                                                                                                                                  
    BEGIN CATCH
                                                                                                                                                                                                                                              
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message, NULL AS OutwardId, NULL AS OutwardDcNo;
                                                                                                                                                             
    END CATCH
                                                                                                                                                                                                                                                
END
                                                                                                                                                                                                                                                          
