Text                                                                                                                                                                                                                                                           
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                                                                                                                                                                                                                                                             
-- 5. Recreate sp_UpdateOutwardSizeCounts
                                                                                                                                                                                                                    
CREATE PROCEDURE sp_UpdateOutwardSizeCounts
                                                                                                                                                                                                                  
    @OutwardId INT,
                                                                                                                                                                                                                                          
    @SizeCounts OutwardSizeCountType READONLY
                                                                                                                                                                                                                
AS
                                                                                                                                                                                                                                                           
BEGIN
                                                                                                                                                                                                                                                        
    SET NOCOUNT ON;
                                                                                                                                                                                                                                          
    
                                                                                                                                                                                                                                                         
    -- Delete existing sizes for this OutwardId
                                                                                                                                                                                                              
    DELETE FROM OutwardSizeCount
                                                                                                                                                                                                                             
    WHERE OutwardId = @OutwardId;
                                                                                                                                                                                                                            

                                                                                                                                                                                                                                                             
    -- Insert new sizes
                                                                                                                                                                                                                                      
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
                                                                                                                                                                                                                                                          
