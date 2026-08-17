-- =============================================
-- Script to create Employee tables and SPs
-- =============================================

USE [SSManagementDEV];
GO

-- 1. Create Tables
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmployeeDetails')
BEGIN
    CREATE TABLE EmployeeDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId NVARCHAR(50) NOT NULL UNIQUE,
        FullName NVARCHAR(150) NOT NULL,
        Gender NVARCHAR(20) NULL,
        Dob DATE NULL,
        MobileNumber NVARCHAR(20) NULL,
        Designation NVARCHAR(100) NULL,
        JoiningDate DATE NULL,
        MonthlySalary DECIMAL(18,2) NULL,
        DailySalary DECIMAL(18,2) NULL,
        Incentive DECIMAL(18,2) NULL,
        BankName NVARCHAR(150) NULL,
        AccountNumber NVARCHAR(50) NULL,
        IfscCode NVARCHAR(50) NULL,
        CreatedDate DATETIME DEFAULT GETDATE(),
        IsActive BIT DEFAULT 1
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmployeeAttendance')
BEGIN
    CREATE TABLE EmployeeAttendance (
        AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId NVARCHAR(50) NOT NULL,
        AttendanceDate DATE NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        Remarks NVARCHAR(250) NULL,
        CreatedDate DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_EmployeeAttendance_Employee FOREIGN KEY (EmployeeId) REFERENCES EmployeeDetails(EmployeeId)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmployeePayroll')
BEGIN
    CREATE TABLE EmployeePayroll (
        PayrollId INT IDENTITY(1,1) PRIMARY KEY,
        EmployeeId NVARCHAR(50) NOT NULL,
        PayrollMonth INT NOT NULL,
        PayrollYear INT NOT NULL,
        PresentDays INT NULL,
        DailySalary DECIMAL(18,2) NULL,
        Incentive DECIMAL(18,2) NULL,
        TotalSalary DECIMAL(18,2) NULL,
        IsPaid BIT DEFAULT 0,
        CreatedDate DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_EmployeePayroll_Employee FOREIGN KEY (EmployeeId) REFERENCES EmployeeDetails(EmployeeId)
    );
END
GO

-- 2. Stored Procedures
IF OBJECT_ID('sp_ManageEmployee', 'P') IS NOT NULL DROP PROCEDURE sp_ManageEmployee;
GO
CREATE PROCEDURE sp_ManageEmployee
    @Id INT = 0,
    @EmployeeId NVARCHAR(50),
    @FullName NVARCHAR(150),
    @Gender NVARCHAR(20) = NULL,
    @Dob DATE = NULL,
    @MobileNumber NVARCHAR(20) = NULL,
    @Designation NVARCHAR(100) = NULL,
    @JoiningDate DATE = NULL,
    @MonthlySalary DECIMAL(18,2) = NULL,
    @DailySalary DECIMAL(18,2) = NULL,
    @Incentive DECIMAL(18,2) = NULL,
    @BankName NVARCHAR(150) = NULL,
    @AccountNumber NVARCHAR(50) = NULL,
    @IfscCode NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM EmployeeDetails WHERE EmployeeId = @EmployeeId AND Id != @Id)
    BEGIN
        SELECT 0 AS Id, 'EmployeeId already exists' AS Message, CAST(0 AS BIT) AS Status;
        RETURN;
    END

    IF @Id = 0 OR NOT EXISTS (SELECT 1 FROM EmployeeDetails WHERE Id = @Id)
    BEGIN
        INSERT INTO EmployeeDetails (EmployeeId, FullName, Gender, Dob, MobileNumber, Designation, JoiningDate, MonthlySalary, DailySalary, Incentive, BankName, AccountNumber, IfscCode)
        VALUES (@EmployeeId, @FullName, @Gender, @Dob, @MobileNumber, @Designation, @JoiningDate, @MonthlySalary, @DailySalary, @Incentive, @BankName, @AccountNumber, @IfscCode);
        
        SELECT SCOPE_IDENTITY() AS Id, 'Employee created successfully' AS Message, CAST(1 AS BIT) AS Status;
    END
    ELSE
    BEGIN
        UPDATE EmployeeDetails
        SET EmployeeId = @EmployeeId,
            FullName = @FullName,
            Gender = @Gender,
            Dob = @Dob,
            MobileNumber = @MobileNumber,
            Designation = @Designation,
            JoiningDate = @JoiningDate,
            MonthlySalary = @MonthlySalary,
            DailySalary = @DailySalary,
            Incentive = @Incentive,
            BankName = @BankName,
            AccountNumber = @AccountNumber,
            IfscCode = @IfscCode
        WHERE Id = @Id;

        SELECT @Id AS Id, 'Employee updated successfully' AS Message, CAST(1 AS BIT) AS Status;
    END
END
GO

IF OBJECT_ID('sp_GetAllEmployees', 'P') IS NOT NULL DROP PROCEDURE sp_GetAllEmployees;
GO
CREATE PROCEDURE sp_GetAllEmployees
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM EmployeeDetails WHERE IsActive = 1 ORDER BY FullName;
END
GO

IF OBJECT_ID('sp_GetEmployeeById', 'P') IS NOT NULL DROP PROCEDURE sp_GetEmployeeById;
GO
CREATE PROCEDURE sp_GetEmployeeById
    @Id NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM EmployeeDetails 
    WHERE (CAST(Id AS NVARCHAR(50)) = @Id OR EmployeeId = @Id) AND IsActive = 1;
END
GO

IF OBJECT_ID('sp_DeleteEmployee', 'P') IS NOT NULL DROP PROCEDURE sp_DeleteEmployee;
GO
CREATE PROCEDURE sp_DeleteEmployee
    @Id NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE EmployeeDetails SET IsActive = 0 WHERE (CAST(Id AS NVARCHAR(50)) = @Id OR EmployeeId = @Id);
    SELECT 1 AS Id, 'Employee deleted successfully' AS Message, CAST(1 AS BIT) AS Status;
END
GO

IF OBJECT_ID('sp_SaveAttendance', 'P') IS NOT NULL DROP PROCEDURE sp_SaveAttendance;
GO
CREATE PROCEDURE sp_SaveAttendance
    @AttendanceId INT = 0,
    @EmployeeId NVARCHAR(50),
    @Date DATE,
    @Status NVARCHAR(20),
    @Remarks NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM EmployeeAttendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Date AND AttendanceId != @AttendanceId)
    BEGIN
        UPDATE EmployeeAttendance SET Status = @Status, Remarks = @Remarks WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Date;
        SELECT AttendanceId AS Id, 'Attendance updated' AS Message, CAST(1 AS BIT) AS Status FROM EmployeeAttendance WHERE EmployeeId = @EmployeeId AND AttendanceDate = @Date;
    END
    ELSE IF @AttendanceId = 0
    BEGIN
        INSERT INTO EmployeeAttendance (EmployeeId, AttendanceDate, Status, Remarks)
        VALUES (@EmployeeId, @Date, @Status, @Remarks);
        SELECT SCOPE_IDENTITY() AS Id, 'Attendance saved' AS Message, CAST(1 AS BIT) AS Status;
    END
    ELSE
    BEGIN
        UPDATE EmployeeAttendance SET Status = @Status, Remarks = @Remarks WHERE AttendanceId = @AttendanceId;
        SELECT @AttendanceId AS Id, 'Attendance updated' AS Message, CAST(1 AS BIT) AS Status;
    END
END
GO

IF OBJECT_ID('sp_GetAttendanceByDate', 'P') IS NOT NULL DROP PROCEDURE sp_GetAttendanceByDate;
GO
CREATE PROCEDURE sp_GetAttendanceByDate
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT A.*, E.FullName 
    FROM EmployeeAttendance A
    INNER JOIN EmployeeDetails E ON A.EmployeeId = E.EmployeeId
    WHERE A.AttendanceDate = @Date;
END
GO

IF OBJECT_ID('sp_GetAttendanceByMonth', 'P') IS NOT NULL DROP PROCEDURE sp_GetAttendanceByMonth;
GO
CREATE PROCEDURE sp_GetAttendanceByMonth
    @EmployeeId NVARCHAR(50),
    @Date DATE -- The exact parameter the frontend passes, maybe e.g. '2026-05-01'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Month INT = MONTH(@Date);
    DECLARE @Year INT = YEAR(@Date);

    SELECT A.*, E.FullName 
    FROM EmployeeAttendance A
    INNER JOIN EmployeeDetails E ON A.EmployeeId = E.EmployeeId
    WHERE A.EmployeeId = @EmployeeId AND MONTH(A.AttendanceDate) = @Month AND YEAR(A.AttendanceDate) = @Year;
END
GO

IF OBJECT_ID('sp_GeneratePayroll', 'P') IS NOT NULL DROP PROCEDURE sp_GeneratePayroll;
GO
CREATE PROCEDURE sp_GeneratePayroll
    @PayrollId INT = 0,
    @EmployeeId NVARCHAR(50),
    @Month INT,
    @Year INT,
    @PresentDays INT,
    @DailySalary DECIMAL(18,2),
    @Incentive DECIMAL(18,2),
    @TotalSalary DECIMAL(18,2),
    @IsPaid BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM EmployeePayroll WHERE EmployeeId = @EmployeeId AND PayrollMonth = @Month AND PayrollYear = @Year AND PayrollId != @PayrollId)
    BEGIN
        UPDATE EmployeePayroll 
        SET PresentDays = @PresentDays, DailySalary = @DailySalary, Incentive = @Incentive, TotalSalary = @TotalSalary, IsPaid = @IsPaid
        WHERE EmployeeId = @EmployeeId AND PayrollMonth = @Month AND PayrollYear = @Year;
        
        SELECT PayrollId AS Id, 'Payroll updated' AS Message, CAST(1 AS BIT) AS Status FROM EmployeePayroll WHERE EmployeeId = @EmployeeId AND PayrollMonth = @Month AND PayrollYear = @Year;
    END
    ELSE IF @PayrollId = 0
    BEGIN
        INSERT INTO EmployeePayroll (EmployeeId, PayrollMonth, PayrollYear, PresentDays, DailySalary, Incentive, TotalSalary, IsPaid)
        VALUES (@EmployeeId, @Month, @Year, @PresentDays, @DailySalary, @Incentive, @TotalSalary, @IsPaid);
        SELECT SCOPE_IDENTITY() AS Id, 'Payroll saved' AS Message, CAST(1 AS BIT) AS Status;
    END
    ELSE
    BEGIN
        UPDATE EmployeePayroll 
        SET PresentDays = @PresentDays, DailySalary = @DailySalary, Incentive = @Incentive, TotalSalary = @TotalSalary, IsPaid = @IsPaid
        WHERE PayrollId = @PayrollId;
        SELECT @PayrollId AS Id, 'Payroll updated' AS Message, CAST(1 AS BIT) AS Status;
    END
END
GO

IF OBJECT_ID('sp_GetPayrollByMonth', 'P') IS NOT NULL DROP PROCEDURE sp_GetPayrollByMonth;
GO
CREATE PROCEDURE sp_GetPayrollByMonth
    @Month INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT P.*, E.FullName 
    FROM EmployeePayroll P
    INNER JOIN EmployeeDetails E ON P.EmployeeId = E.EmployeeId
    WHERE P.PayrollMonth = @Month AND P.PayrollYear = @Year;
END
GO

IF OBJECT_ID('sp_GetPayrollSummary', 'P') IS NOT NULL DROP PROCEDURE sp_GetPayrollSummary;
GO
CREATE PROCEDURE sp_GetPayrollSummary
    @Month INT,
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        E.EmployeeId, 
        E.FullName,
        E.MonthlySalary,
        E.DailySalary,
        P.PresentDays,
        P.Incentive,
        P.TotalSalary,
        P.IsPaid
    FROM EmployeeDetails E
    LEFT JOIN EmployeePayroll P ON E.EmployeeId = P.EmployeeId AND P.PayrollMonth = @Month AND P.PayrollYear = @Year
    WHERE E.IsActive = 1;
END
GO
