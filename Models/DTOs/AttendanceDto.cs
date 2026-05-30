using System;

namespace SSProjectSolution.Models.DTOs
{
    public class AttendanceDto
    {
        public int? AttendanceId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}
