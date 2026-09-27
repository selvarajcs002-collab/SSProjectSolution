using System;

namespace SSProjectSolution.Request
{
    public class UpdateAdvanceAmountDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
    }
}
