namespace SSProjectSolution.Models
{
    public class InwardSizeCountModel
    {
        public int Id { get; set; }
        public int InwardId { get; set; }
        public string StyleNo { get; set; } = string.Empty;
        public string DesignName { get; set; } = string.Empty;
        public string Colour { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
