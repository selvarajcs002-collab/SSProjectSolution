Text                                                                                                                                                                                                                                                           
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                                                                                                                                                                                                                                                             
-- =============================================
                                                                                                                                                                                                             
-- Author:		Antigravity
                                                                                                                                                                                                                                      
-- Create date: 2026-07-01
                                                                                                                                                                                                                                   
-- Description:	Get Last Transactions
                                                                                                                                                                                                                        
-- =============================================
                                                                                                                                                                                                             
CREATE   PROCEDURE [dbo].[usp_GetLastTransactions]
                                                                                                                                                                                                           
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
                                                                                                                                                                                                                                
            I.CreatedDate AS [Date],
                                                                                                                                                                                                                         
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
                                                                                                                                                                                                                                                        
    SELECT TOP (@TopCount) *
                                                                                                                                                                                                                                 
    FROM AllTransactions
                                                                                                                                                                                                                                     
    ORDER BY [Date] DESC;
                                                                                                                                                                                                                                    
END
                                                                                                                                                                                                                                                          
