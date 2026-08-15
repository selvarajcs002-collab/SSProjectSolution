using System.Collections.Generic;

namespace SSProjectSolution.Request
{
    public class OutwardRequest
    {
        public OutwardData? Outward { get; set; }
        public List<SizeData>? Sizes { get; set; } // Legacy backward compatibility
        public List<ColourBreakdown>? ColourBreakdowns { get; set; }
    }

    public class ColourBreakdown
    {
        public string? ColourId { get; set; }
        public string? ColourName { get; set; }
        public int? ColourTotal { get; set; }
        public List<SizeBreakdown>? SizeBreakdowns { get; set; }
        
        // Legacy/fallback properties
        public string? Colour { get; set; }
        public List<SizeData>? Sizes { get; set; }
    }

    public class SizeBreakdown
    {
        public string? SizeId { get; set; }
        public string? SizeName { get; set; }
        public int AvailableQty { get; set; }
        public int Quantity { get; set; }
    }

    public class OutwardData
    {
        public int OutwardId { get; set; }
        public string? Mode { get; set; }
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
    }

    public class SizeData
    {
        public string? Size { get; set; }
        public int Count { get; set; }
    }
}
