using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSProjectSolution.Repositories
{
    public interface IEmployeeRepository
    {
        Task<CommonResponse> ManageEmployeeAsync(EmployeeDto employee);
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto> GetEmployeeByIdAsync(string id);
        Task<CommonResponse> DeleteEmployeeAsync(string id);
        Task<CommonResponse> SaveAttendanceAsync(AttendanceDto attendance);
        Task<IEnumerable<AttendanceDto>> GetAttendanceByDateAsync(DateTime date);
        Task<IEnumerable<AttendanceDto>> GetAttendanceByMonthAsync(string employeeId, DateTime date);
        Task<CommonResponse> GeneratePayrollAsync(PayrollDto payroll);
        Task<IEnumerable<PayrollDto>> GetPayrollByMonthAsync(int month, int year);
        Task<IEnumerable<object>> GetPayrollSummaryAsync(int month, int year);
    }
}
