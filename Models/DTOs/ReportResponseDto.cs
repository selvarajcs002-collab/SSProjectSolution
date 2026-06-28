using System.Collections.Generic;

namespace SSProjectSolution.Models.DTOs
{
    public class ReportResponseDto
    {
        public ReportSummaryDto Summary { get; set; }
        public List<string> DynamicColumns { get; set; }
        public List<ReportDataRowDto> Data { get; set; }
    }

    public class ReportSummaryDto
    {
        public int TotalRecords { get; set; }
        public int TotalBitsCount { get; set; }
        public decimal TotalMeter { get; set; }
    }

    public class ReportDataRowDto
    {
        public int Sno { get; set; }
        public string DcNo { get; set; }
        public string Date { get; set; }
        public string StyleNo { get; set; }
        public string DesignName { get; set; }
        public string Colour { get; set; }
        public int TotalBits { get; set; }
        public decimal TotalMeter { get; set; }
        public Dictionary<string, decimal> DynamicValues { get; set; } = new Dictionary<string, decimal>();
    }
}
