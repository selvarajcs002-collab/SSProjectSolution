using System;

namespace SSProjectSolution.Request
{
    public class InwardOutwardFilterRequest
    {
        public string? Mode { get; set; }           // INWARD / OUTWARD / null
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CompanyId { get; set; }
        public string? StyleNo { get; set; }
        public string? DesignName { get; set; }
    }
}
