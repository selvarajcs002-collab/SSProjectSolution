using System.Collections.Generic;

namespace SSProjectSolution.Request
{
    public class GenerateDcRequest
    {
        public string DcNo { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string GstNo { get; set; } = string.Empty;
        public string JobReference { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string PrinterName { get; set; } = string.Empty;
        public string PrintedBy { get; set; } = string.Empty;

        public List<DcItem> Items { get; set; } = new List<DcItem>();
        
        // New fields for Size Based
        public string Style { get; set; } = string.Empty;
        public string DesignReference { get; set; } = string.Empty;
        public string ItemType { get; set; } = "Size Based";
        public bool LotCompleted { get; set; } = false;

        public List<DcColourBreakdown> ColourBreakdowns { get; set; } = new List<DcColourBreakdown>();
    }

    public class DcItem
    {
        public int SrNo { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Uom { get; set; } = string.Empty;
    }

    public class DcColourBreakdown
    {
        public string ColourName { get; set; }

        public List<DcSizeBreakdown> Sizes { get; set; }
            = new();
    }

    public class DcSizeBreakdown
    {
        public string SizeName { get; set; }

        public int Quantity { get; set; }
    }
}
