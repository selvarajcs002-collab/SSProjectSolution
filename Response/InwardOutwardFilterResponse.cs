using System;
using System.Collections.Generic;

namespace SSProjectSolution.Response
{
    public class InwardOutwardNestedResponse
    {
        public List<InwardOutwardItem> Inward { get; set; } = new();
        public List<InwardOutwardItem> Outward { get; set; } = new();
    }

    public class InwardOutwardItem
    {
        public int Id { get; set; }
        public string? CompanyName { get; set; }
        public int? CompanyId { get; set; }
        public string? Colour { get; set; }
        public string? DesignName { get; set; }
        public string? StyleNo { get; set; }
        public string? UploadURL { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? DcNo { get; set; }
        public string? Status { get; set; }
        public List<SizeCountDto> SizeCounts { get; set; } = new();
    }

    public class SizeCountDto
    {
        public int? SizeCountId { get; set; }
        public string? Size { get; set; }
        public int? Count { get; set; }
    }
}
