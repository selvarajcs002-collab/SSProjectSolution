using System.Collections.Generic;

namespace SSProjectSolution.Request
{
    public class OutwardUpdateRequest
    {
        public int OutwardId { get; set; }
        public int CompanyId { get; set; }
        public string? Colour { get; set; }
        public string? DesignName { get; set; }
        public string? StyleNo { get; set; }
        public string? UploadURL { get; set; }
        public string? CreatedBy { get; set; }
        public string? Status { get; set; }
        public string? DeliveryTo { get; set; }
        public string? PoNo { get; set; }
        public string? Weight { get; set; }
        public string? NoOfBundles { get; set; }
        public string? Remarks { get; set; }
        public List<string>? SelectedDcNos { get; set; }
        public List<SizeCountUpdateDto>? SizeCounts { get; set; }
        public List<ColourBreakdown>? ColourBreakdowns { get; set; }
    }

    public class SizeCountUpdateDto
    {
        public string? Size { get; set; }
        public int Count { get; set; }
    }
}
