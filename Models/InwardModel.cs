namespace SSProjectSolution.Models
{
    public class InwardModel
    {
        public int InwardId { get; set; }
        public int CompanyId { get; set; }
        public string Colour { get; set; } = string.Empty;
        public string DesignName { get; set; } = string.Empty;
        public string StyleNo { get; set; } = string.Empty;
        public string? UploadURL { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
