using System;

namespace SSProjectSolution.Request
{
    public class StatusFilterRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CompanyId { get; set; }
        public string? StyleId { get; set; }
        public string? DesignId { get; set; }
        public string TransactionType { get; set; } = "INWARD";
        public string ViewType { get; set; } = "SIZE";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; } = "Date";
        public string? SortDirection { get; set; } = "DESC";
    }
}
