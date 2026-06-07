using System;
using System.Collections.Generic;

namespace SSProjectSolution.Response
{
    public class ActivityLogResponse
    {
        public List<ActivityLogItem> Data { get; set; } = new List<ActivityLogItem>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
        public ActivityLogSummary Summary { get; set; } = new ActivityLogSummary();
    }

    public class ActivityLogSummary
    {
        public decimal TotalBitsCount { get; set; }
        public decimal TotalMeter { get; set; }
    }

    public class ActivityLogItem
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? DcNo { get; set; }
        public DateTime Date { get; set; }
        public string? StyleNo { get; set; }
        public string? DesignName { get; set; }
        public string? Colour { get; set; }
        public decimal TotalBitsCount { get; set; }
        public decimal TotalMeter { get; set; }
        public List<ActivityLogDetail> Details { get; set; } = new List<ActivityLogDetail>();
    }

    public class ActivityLogDetail
    {
        public long Id { get; set; }
        public long ParentId { get; set; }
        
        // For Size based
        public string? Size { get; set; }
        public int? Count { get; set; }

        // For Meter based
        public decimal? MeterValue { get; set; }
        public decimal? BitsCount { get; set; }
        public decimal? TotalMeter { get; set; }
    }
}
