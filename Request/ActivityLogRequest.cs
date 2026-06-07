using System;

namespace SSProjectSolution.Request
{
    public class ActivityLogRequest
    {
        public string? Module { get; set; } // 'INWARD' or 'OUTWARD'
        public string? ViewType { get; set; } // 'S' or 'M'
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? CompanyId { get; set; }
        public string? StyleNo { get; set; }
        public string? DesignName { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; } = "Date";
        public string? SortDirection { get; set; } = "DESC";
    }
}
