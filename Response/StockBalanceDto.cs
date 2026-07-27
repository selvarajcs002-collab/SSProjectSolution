namespace SSProjectSolution.Response
{
    public class StockBalanceDto
    {
        public string Size { get; set; }
        public int TotalInward { get; set; }
        public int TotalOutward { get; set; }
        public int Available { get; set; }
        public int Difference { get; set; }
    }
}
