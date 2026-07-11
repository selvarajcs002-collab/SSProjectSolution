using System;

namespace SSProjectSolution.Response
{
    public class StockTransactionDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string DcNo { get; set; }
        public string CompanyName { get; set; }
        public string StyleNo { get; set; }
        public string DesignName { get; set; }
        public string Color { get; set; }
        public int? InwardQty { get; set; }
        public int? OutwardQty { get; set; }
    }
}
