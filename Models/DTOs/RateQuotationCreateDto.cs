namespace SSProjectSolution.Models.DTOs
{
    public class RateQuotationCreateDto
    {
        public DateTime QuotationDate { get; set; }
        public long CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? MobileNo { get; set; }
        public string? EmailId { get; set; }
        public string? Address { get; set; }
        public string? StyleNo { get; set; }
        public string? DesignName { get; set; }
        public string? ProductType { get; set; }
        public string? RatePerPiece { get; set; }
        public string? RatePerMeter { get; set; }
        public string? NoOfStitches { get; set; }
        public int? ChenilleColors { get; set; }
        public int? NormalEmbColors { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remarks { get; set; }
        public string Status { get; set; } = "Draft";
        public long CreatedBy { get; set; }
    }
}
