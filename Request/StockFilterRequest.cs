using System;

namespace SSProjectSolution.Request
{
    public class StockFilterRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CompanyId { get; set; }
        public string? StyleNo { get; set; }
        public string? DesignName { get; set; }
        public string? Colour { get; set; }
    }
}
