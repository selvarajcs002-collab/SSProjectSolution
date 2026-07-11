Text                                                                                                                                                                                                                                                           
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
CREATE   PROCEDURE [dbo].[usp_GetInwardOutwardDetails_Filter]
                                                                                                                                                                                                
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
                                                                                                                                                                                                                                     
            I.CreatedDate,
                                                                                                                                                                                                                                   
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
                                                                                                                                                                                                                                                          
