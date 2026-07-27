Text                                                                                                                                                                                                                                                           
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE SP_SAVE_INWARD_METER
                                                                                                                                                                                                                        
    @InwardId INT,
                                                                                                                                                                                                                                           
    @CompanyId INT,
                                                                                                                                                                                                                                          
    @Colour NVARCHAR(100),
                                                                                                                                                                                                                                   
    @DesignName NVARCHAR(150),
                                                                                                                                                                                                                               
    @StyleNo NVARCHAR(100),
                                                                                                                                                                                                                                  
    @InwardDcNo NVARCHAR(100),
                                                                                                                                                                                                                               
    @PoNo NVARCHAR(100) = NULL,
                                                                                                                                                                                                                              
    @EntryType CHAR(1) = 'M',
                                                                                                                                                                                                                                
    @CreatedBy INT,
                                                                                                                                                                                                                                          
    @MeterDetails MeterDetailType READONLY
                                                                                                                                                                                                                   
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
                                                                                                                                                                                                                                  
                CreatedDate
                                                                                                                                                                                                                                  
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
                                                                                                                                                                                                                                 
                GETDATE()
                                                                                                                                                                                                                                    
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
                                                                                                                                                                                                                                
                UpdatedDate = GETDATE()
                                                                                                                                                                                                                      
            WHERE InwardId = @CurrentInwardId AND CompanyId = @CompanyId;
                                                                                                                                                                                    
        END
                                                                                                                                                                                                                                                  

                                                                                                                                                                                                                                                             
        -- Delete existing meter details
                                                                                                                                                                                                                     
        DELETE FROM INWARD_METER_DETAIL WHERE IMD_INWARD_ID = @CurrentInwardId;
                                                                                                                                                                              

                                                                                                                                                                                                                                                             
        -- Insert new meter details
                                                                                                                                                                                                                          
        -- Backend recalculates Total Meter = Meter Value * Bits Count
                                                                                                                                                                                       
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
                                                                                                                                                                                                                                                          
