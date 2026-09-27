using System;

namespace SSProjectSolution.Models
{
    public class AdvanceAmount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public string? ChallanNo { get; set; }
        public string? ChallanFileName { get; set; }
        public string? ChallanFilePath { get; set; }
    }
}
