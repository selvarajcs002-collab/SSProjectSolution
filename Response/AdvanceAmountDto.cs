using System;

namespace SSProjectSolution.Response
{
    public class AdvanceAmountDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ChallanNo { get; set; }
        public string? ChallanFileName { get; set; }
    }
}
