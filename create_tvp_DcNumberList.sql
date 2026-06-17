-- ============================================================
-- SQL SETUP: Table-Valued Parameter type for DC number list
-- Run this script ONCE against your SQL Server database.
-- ============================================================

-- 1. Create the TVP user-defined table type
--    (used by DcDetailRepository to pass multiple DC numbers
--     in a single parameterised call — no dynamic SQL, no injection risk)
IF NOT EXISTS (
    SELECT 1 FROM sys.types
    WHERE is_table_type = 1
      AND name = 'DcNumberList'
      AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TYPE dbo.DcNumberList AS TABLE
    (
        DcNo NVARCHAR(100) NOT NULL
    );

    PRINT 'Created type dbo.DcNumberList';
END
ELSE
BEGIN
    PRINT 'Type dbo.DcNumberList already exists — skipping creation.';
END
GO

-- ============================================================
-- VERIFICATION QUERIES  (run to confirm setup)
-- ============================================================

-- Confirm the type was created
SELECT 
    name          AS TypeName,
    is_table_type AS IsTableType,
    create_date   AS CreatedAt
FROM sys.types
WHERE name = 'DcNumberList';
GO

-- ============================================================
-- HOW THE TYPE IS USED IN C# (DcDetailRepository):
-- ============================================================
--
--   var dcTable = new DataTable();
--   dcTable.Columns.Add("DcNo", typeof(string));
--   foreach (var dc in inwardDcNos) dcTable.Rows.Add(dc);
--
--   var parameters = new DynamicParameters();
--   parameters.Add("CompanyId", companyId);
--   parameters.Add("DcList", dcTable.AsTableValuedParameter("dbo.DcNumberList"));
--
--   await connection.QueryAsync(sql, parameters);
--
-- The SQL query then uses:
--   INNER JOIN @DcList dl ON dl.DcNo = i.InwardDcNo
-- ============================================================
