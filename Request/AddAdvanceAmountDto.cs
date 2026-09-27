using System;

namespace SSProjectSolution.Request
{
    public class AddAdvanceAmountDto
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
    }
}
