CREATE OR ALTER PROCEDURE [dbo].[sp_EMP_DeleteDailyProduction]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM EMP_DailyProductions WHERE Id = @Id)
    BEGIN
        DELETE FROM EMP_DailyProductions WHERE Id = @Id;
        SELECT @Id;
    END
    ELSE
    BEGIN
        SELECT 0;
    END
END
GO
