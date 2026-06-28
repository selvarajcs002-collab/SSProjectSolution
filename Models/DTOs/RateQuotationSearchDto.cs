namespace SSProjectSolution.Models.DTOs
{
    public class RateQuotationSearchDto
    {
        public string? QuotationNo { get; set; }
        public string? CompanyName { get; set; }
        public string? StyleNo { get; set; }
        public string? DesignName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
