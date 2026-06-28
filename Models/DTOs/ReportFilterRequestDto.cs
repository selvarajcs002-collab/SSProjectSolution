namespace SSProjectSolution.Models.DTOs
{
    public class ReportFilterRequestDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Mode { get; set; }
        public string? Type { get; set; }
        public int? CompanyId { get; set; }
        public string? StyleNo { get; set; }
        public string? DesignName { get; set; }
    }
}
