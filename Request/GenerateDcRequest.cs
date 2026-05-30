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
    }

    public class DcItem
    {
        public int SrNo { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Uom { get; set; } = string.Empty;
    }
}
