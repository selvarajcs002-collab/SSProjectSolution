using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Repositories;
using SSProjectSolution.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<CommonResponse> ManageEmployeeAsync(EmployeeDto employee)
        {
            return await _employeeRepository.ManageEmployeeAsync(employee);
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            return await _employeeRepository.GetAllEmployeesAsync();
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(string id)
        {
            return await _employeeRepository.GetEmployeeByIdAsync(id);
        }

        public async Task<CommonResponse> DeleteEmployeeAsync(string id)
        {
            return await _employeeRepository.DeleteEmployeeAsync(id);
        }

        public async Task<CommonResponse> SaveAttendanceAsync(AttendanceDto attendance)
        {
            return await _employeeRepository.SaveAttendanceAsync(attendance);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByDateAsync(DateTime date)
        {
            return await _employeeRepository.GetAttendanceByDateAsync(date);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByMonthAsync(string employeeId, DateTime date)
        {
            return await _employeeRepository.GetAttendanceByMonthAsync(employeeId, date);
        }

        public async Task<CommonResponse> GeneratePayrollAsync(PayrollDto payroll)
        {
            // Basic business logic if needed before persisting.
            payroll.TotalSalary = (payroll.DailySalary * payroll.PresentDays) + payroll.Incentive;
            return await _employeeRepository.GeneratePayrollAsync(payroll);
        }

        public async Task<IEnumerable<PayrollDto>> GetPayrollByMonthAsync(int month, int year)
        {
            return await _employeeRepository.GetPayrollByMonthAsync(month, year);
        }

        public async Task<IEnumerable<object>> GetPayrollSummaryAsync(int month, int year)
        {
            return await _employeeRepository.GetPayrollSummaryAsync(month, year);
        }
    }
}
