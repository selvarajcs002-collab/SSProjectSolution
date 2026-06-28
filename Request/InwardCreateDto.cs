using System.ComponentModel.DataAnnotations;

namespace SSProjectSolution.Request
{
    public class InwardCreateDto
    {
        [Required(ErrorMessage = "CompanyId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CompanyId must be greater than 0")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Colour is required")]
        [StringLength(10000)]
        public string Colour { get; set; } = string.Empty;

        [Required(ErrorMessage = "DesignName is required")]
        [StringLength(10000)]
        public string DesignName { get; set; } = string.Empty;

        [Required(ErrorMessage = "StyleNo is required")]
        [StringLength(10000)]
        public string StyleNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "InwardDcNo is required")]
        [StringLength(10000)]
        public string InwardDcNo { get; set; } = string.Empty;

        [StringLength(10000)]
        public string? PoNo { get; set; }

        public string? UploadURL { get; set; } = null;

        [Required(ErrorMessage = "CreatedBy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CreatedBy must be a valid user Id")]
        public int CreatedBy { get; set; }
    }
}
