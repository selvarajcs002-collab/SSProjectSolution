namespace SSProjectSolution.Response
{
    public class StockSummaryDto
    {
        public int TotalInwardQty { get; set; }
        public decimal TotalInwardPercent { get; set; }
        public int TotalOutwardQty { get; set; }
        public decimal TotalOutwardPercent { get; set; }
        public int AvailableStock { get; set; }
        public decimal AvailableStockPercent { get; set; }
        public int TodaysInward { get; set; }
        public decimal TodaysInwardPercent { get; set; }
        public int TodaysOutward { get; set; }
        public decimal TodaysOutwardPercent { get; set; }
        public int LowStockItems { get; set; }
    }
}
