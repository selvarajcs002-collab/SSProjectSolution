using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SSProjectSolution.Request
{
    public class InwardMeterSaveRequest
    {
        public int InwardId { get; set; }

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

        [Required]
        public string InwardDcNo { get; set; } = string.Empty;

        public string? PoNo { get; set; }

        public char EntryType { get; set; } = 'M';

        [Required(ErrorMessage = "CreatedBy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CreatedBy must be a valid user Id")]
        public int CreatedBy { get; set; }

        [Required(ErrorMessage = "MeterDetails list cannot be empty")]
        [MinLength(1, ErrorMessage = "At least one meter detail must be provided")]
        public List<MeterDetailDto> MeterDetails { get; set; } = new List<MeterDetailDto>();
    }
}
