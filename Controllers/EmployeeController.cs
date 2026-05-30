using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Response;
using SSProjectSolution.Services;
using System;
using System.Threading.Tasks;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost("manage")]
        public async Task<IActionResult> ManageEmployee([FromBody] EmployeeDto employee)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _employeeService.ManageEmployeeAsync(employee);
                if (result.Status) return Ok(result);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var result = await _employeeService.GetAllEmployeesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(string id)
        {
            try
            {
                var result = await _employeeService.GetEmployeeByIdAsync(id);
                if (result == null) return NotFound(new { message = "Employee not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            try
            {
                var result = await _employeeService.DeleteEmployeeAsync(id);
                if (result.Status) return Ok(result);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }

        [HttpPost("attendance")]
        public async Task<IActionResult> SaveAttendance([FromBody] AttendanceDto attendance)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _employeeService.SaveAttendanceAsync(attendance);
                if (result.Status) return Ok(result);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }

        [HttpGet("attendance/{date}")]
        public async Task<IActionResult> GetAttendanceByDate(DateTime date)
        {
            try
            {
                var result = await _employeeService.GetAttendanceByDateAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("attendance/month/{employeeId}/{date}")]
        public async Task<IActionResult> GetAttendanceByMonth(string employeeId, DateTime date)
        {
            try
            {
                var result = await _employeeService.GetAttendanceByMonthAsync(employeeId, date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("payroll")]
        public async Task<IActionResult> GeneratePayroll([FromBody] PayrollDto payroll)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _employeeService.GeneratePayrollAsync(payroll);
                if (result.Status) return Ok(result);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }

        [HttpGet("payroll/{month}/{year}")]
        public async Task<IActionResult> GetPayrollByMonth(int month, int year)
        {
            try
            {
                var result = await _employeeService.GetPayrollByMonthAsync(month, year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("payroll/summary/{month}/{year}")]
        public async Task<IActionResult> GetPayrollSummary(int month, int year)
        {
            try
            {
                var result = await _employeeService.GetPayrollSummaryAsync(month, year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
