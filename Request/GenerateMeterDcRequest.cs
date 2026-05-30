using System.Collections.Generic;

namespace SSProjectSolution.Request
{
    public class GenerateMeterDcRequest
    {
        public string DcNo { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string GstNo { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string PrintedBy { get; set; } = string.Empty;

        // Job details (from top right section)
        public string Design { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Machine { get; set; } = "Embroidery Machine";
        public string DcType { get; set; } = "Meter Based";

        public List<MeterDcItem> Items { get; set; } = new List<MeterDcItem>();
        public decimal TotalMeterSum { get; set; }
    }

    public class MeterDcItem
    {
        public decimal MeterPerBit { get; set; }
        public decimal BitsCount { get; set; }
        public decimal TotalMeter { get; set; }
    }
}
