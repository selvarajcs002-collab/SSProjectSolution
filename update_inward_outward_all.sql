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
                                                                                                                                                                                                                                                          
