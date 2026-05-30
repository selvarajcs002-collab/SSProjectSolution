namespace SSProjectSolution.Models.DTOs
{
    public class PayrollDto
    {
        public int? PayrollId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int PresentDays { get; set; }
        public decimal DailySalary { get; set; }
        public decimal Incentive { get; set; }
        public decimal TotalSalary { get; set; }
        public bool? IsPaid { get; set; }
    }
}
