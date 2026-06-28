Text                                                                                                                                                                                                                                                           
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

                                                                                                                                                                                                                                                             
CREATE PROCEDURE [dbo].[sp_GetInward_ByCompany_And_DCNo]
                                                                                                                                                                                                     
    @CompanyId INT,
                                                                                                                                                                                                                                          
    @InwardDcNo NVARCHAR(100)
                                                                                                                                                                                                                                
AS
                                                                                                                                                                                                                                                           
BEGIN
                                                                                                                                                                                                                                                        
    SET NOCOUNT ON;
                                                                                                                                                                                                                                          

                                                                                                                                                                                                                                                             
    SELECT 
                                                                                                                                                                                                                                                  
        InwardId AS inward_id,
                                                                                                                                                                                                                               
        CompanyId AS company_id,
                                                                                                                                                                                                                             
        Colour AS colour,
                                                                                                                                                                                                                                    
        DesignName AS design_name,
                                                                                                                                                                                                                           
        StyleNo AS style_no,
                                                                                                                                                                                                                                 
        InwardDcNo AS inward_dc_no
                                                                                                                                                                                                                           
    FROM Inward
                                                                                                                                                                                                                                              
    WHERE CompanyId = @CompanyId
                                                                                                                                                                                                                             
      AND InwardDcNo = @InwardDcNo;
                                                                                                                                                                                                                          
END
                                                                                                                                                                                                                                                          
