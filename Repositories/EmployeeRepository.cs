using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SSProjectSolution.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DapperDBConnection _dbConnection;

        public EmployeeRepository(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<CommonResponse> ManageEmployeeAsync(EmployeeDto employee)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", employee.Id ?? 0);
            parameters.Add("@EmployeeId", employee.EmployeeId);
            parameters.Add("@FullName", employee.FullName);
            parameters.Add("@Gender", employee.Gender);
            parameters.Add("@Dob", employee.Dob);
            parameters.Add("@MobileNumber", employee.MobileNumber);
            parameters.Add("@Designation", employee.Designation);
            parameters.Add("@JoiningDate", employee.JoiningDate);
            parameters.Add("@MonthlySalary", employee.MonthlySalary);
            parameters.Add("@DailySalary", employee.DailySalary);
            parameters.Add("@Incentive", employee.Incentive);
            parameters.Add("@BankName", employee.BankName);
            parameters.Add("@AccountNumber", employee.AccountNumber);
            parameters.Add("@IfscCode", employee.IfscCode);

            return await connection.QueryFirstOrDefaultAsync<CommonResponse>(
                SPConstants.ManageEmployee,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            using var connection = _dbConnection.CreateConnection();
            return await connection.QueryAsync<EmployeeDto>(
                SPConstants.GetAllEmployees,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(string id)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await connection.QueryFirstOrDefaultAsync<EmployeeDto>(
                SPConstants.GetEmployeeById,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<CommonResponse> DeleteEmployeeAsync(string id)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            return await connection.QueryFirstOrDefaultAsync<CommonResponse>(
                SPConstants.DeleteEmployee,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<CommonResponse> SaveAttendanceAsync(AttendanceDto attendance)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@AttendanceId", attendance.AttendanceId ?? 0);
            parameters.Add("@EmployeeId", attendance.EmployeeId);
            parameters.Add("@Date", attendance.Date);
            parameters.Add("@Status", attendance.Status);
            parameters.Add("@Remarks", attendance.Remarks);

            return await connection.QueryFirstOrDefaultAsync<CommonResponse>(
                SPConstants.SaveAttendance,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByDateAsync(DateTime date)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Date", date);

            return await connection.QueryAsync<AttendanceDto>(
                SPConstants.GetAttendanceByDate,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByMonthAsync(string employeeId, DateTime date)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeId", employeeId);
            parameters.Add("@Date", date);

            return await connection.QueryAsync<AttendanceDto>(
                SPConstants.GetAttendanceByMonth,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<CommonResponse> GeneratePayrollAsync(PayrollDto payroll)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PayrollId", payroll.PayrollId ?? 0);
            parameters.Add("@EmployeeId", payroll.EmployeeId);
            parameters.Add("@Month", payroll.Month);
            parameters.Add("@Year", payroll.Year);
            parameters.Add("@PresentDays", payroll.PresentDays);
            parameters.Add("@DailySalary", payroll.DailySalary);
            parameters.Add("@Incentive", payroll.Incentive);
            parameters.Add("@TotalSalary", payroll.TotalSalary);
            parameters.Add("@IsPaid", payroll.IsPaid ?? false);

            return await connection.QueryFirstOrDefaultAsync<CommonResponse>(
                SPConstants.GeneratePayroll,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<PayrollDto>> GetPayrollByMonthAsync(int month, int year)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Month", month);
            parameters.Add("@Year", year);

            return await connection.QueryAsync<PayrollDto>(
                SPConstants.GetPayrollByMonth,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<object>> GetPayrollSummaryAsync(int month, int year)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Month", month);
            parameters.Add("@Year", year);

            return await connection.QueryAsync<dynamic>(
                SPConstants.GetPayrollSummary,
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
