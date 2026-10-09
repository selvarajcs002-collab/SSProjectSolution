using System;

namespace SSProjectSolution.Response
{
    public class ReusableDcNoDto
    {
        public long AllocationId { get; set; }
        public string DcNo { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? OriginalOutwardId { get; set; }
        public int? PreviousOutwardId { get; set; }
        public DateTime? AllocatedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletionReason { get; set; }
        public string? FinancialYear { get; set; }
        public string? Status { get; set; }
    }

    public class ReuseDcNoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? DcNo { get; set; }
        public long? AllocationId { get; set; }
    }

    public class CompanySearchResult
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string GstNo { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Key => CompanyId;
        public string Value => string.IsNullOrWhiteSpace(City)
            ? CompanyName
            : $"{CompanyName} ({City})";
        public string Description => string.IsNullOrWhiteSpace(GstNo) ? City : GstNo;
    }
}
