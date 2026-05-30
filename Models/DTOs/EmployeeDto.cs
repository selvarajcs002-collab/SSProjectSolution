using System;

namespace SSProjectSolution.Models.DTOs
{
    public class EmployeeDto
    {
        public int? Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string? MobileNumber { get; set; }
        public string? Designation { get; set; }
        public DateTime? JoiningDate { get; set; }
        public decimal? MonthlySalary { get; set; }
        public decimal? DailySalary { get; set; }
        public decimal? Incentive { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IfscCode { get; set; }
    }
}
