USE [SSManagementDEV]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[usp_GenerateOutwardDcNo]
(
    @OutwardDcNo NVARCHAR(50) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NextNo INT;
    DECLARE @YearCurrent NVARCHAR(4);
    DECLARE @YearNext NVARCHAR(4);

    ---------------------------------------------------------
    -- CURRENT FINANCIAL YEAR
    ---------------------------------------------------------

    SET @YearCurrent = CAST(YEAR(GETDATE()) AS NVARCHAR(4));
    SET @YearNext    = CAST(YEAR(GETDATE()) + 1 AS NVARCHAR(4));


    ---------------------------------------------------------
    -- GET NEXT DC NUMBER
    --
    -- Start from 1014.
    --
    -- If existing DCs are:
    -- SSE-1012/2026-2027
    -- SSE-1013/2026-2027
    --
    -- next number will be:
    -- SSE-1014/2026-2027
    ---------------------------------------------------------

    SELECT
        @NextNo =
            ISNULL(
                MAX(
                    TRY_CAST(
                        SUBSTRING(
                            OutwardDcNo,
                            5,
                            CHARINDEX('/', OutwardDcNo) - 5
                        ) AS INT
                    )
                ),
                1013
            ) + 1
    FROM dbo.Outward
    WHERE
        OutwardDcNo LIKE 'SSE-%/' + @YearCurrent + '-' + @YearNext
        -- Added this to ignore the rogue '1218' or '1219' test entries in your database
        AND TRY_CAST(SUBSTRING(OutwardDcNo, 5, CHARINDEX('/', OutwardDcNo) - 5) AS INT) < 1200;


    ---------------------------------------------------------
    -- GENERATE DC NUMBER
    ---------------------------------------------------------

    SET @OutwardDcNo =
        CONCAT(
            'SSE-',
            RIGHT('0000' + CAST(@NextNo AS VARCHAR(10)), 4),
            '/',
            @YearCurrent,
            '-',
            @YearNext
        );

END;
