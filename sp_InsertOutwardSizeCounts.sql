Text                                                                                                                                                                                                                                                           
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                                                                                                                                                                                                                                                             
-- 4. Recreate sp_InsertOutwardSizeCounts
                                                                                                                                                                                                                    
CREATE PROCEDURE sp_InsertOutwardSizeCounts
                                                                                                                                                                                                                  
    @OutwardId INT,
                                                                                                                                                                                                                                          
    @SizeCounts OutwardSizeCountType READONLY
                                                                                                                                                                                                                
AS
                                                                                                                                                                                                                                                           
BEGIN
                                                                                                                                                                                                                                                        
    SET NOCOUNT ON;
                                                                                                                                                                                                                                          

                                                                                                                                                                                                                                                             
    -- Insert multiple rows into OutwardSizeCount
                                                                                                                                                                                                            
    INSERT INTO OutwardSizeCount (
                                                                                                                                                                                                                           
        OutwardId,
                                                                                                                                                                                                                                           
        Colour,
                                                                                                                                                                                                                                              
        Size, 
                                                                                                                                                                                                                                               
        Count
                                                                                                                                                                                                                                                
    )
                                                                                                                                                                                                                                                        
    SELECT 
                                                                                                                                                                                                                                                  
        @OutwardId, 
                                                                                                                                                                                                                                         
        LTRIM(RTRIM(Colour)),
                                                                                                                                                                                                                                
        LTRIM(RTRIM(Size)), 
                                                                                                                                                                                                                                 
        Count
                                                                                                                                                                                                                                                
    FROM @SizeCounts
                                                                                                                                                                                                                                         
    WHERE Size IS NOT NULL AND LTRIM(RTRIM(Size)) <> '';
                                                                                                                                                                                                     
END
                                                                                                                                                                                                                                                          
