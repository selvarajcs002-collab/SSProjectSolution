using System;

namespace SSProjectSolution.Request
{
    public class InwardWhatsAppRequest
    {
        public int InwardId { get; set; }
        public string StyleNo { get; set; }
        public string DesignName { get; set; }
        public string Colour { get; set; }
        public string PoNo { get; set; }
        public string InwardDcNo { get; set; }
        public int Quantity { get; set; }
        public DateTime? Date { get; set; }
    }

    public class OutwardWhatsAppRequest
    {
        public int OutwardId { get; set; }
        public string StyleNo { get; set; }
        public string DesignName { get; set; }
        public string Colour { get; set; }
        public string PoNo { get; set; }
        public string OutwardDcNo { get; set; }
        public int Quantity { get; set; }
        public DateTime? Date { get; set; }
    }

    public class WhatsAppResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
