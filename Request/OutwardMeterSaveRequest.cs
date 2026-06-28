using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SSProjectSolution.Request
{
    public class OutwardMeterSaveRequest
    {
        /// <summary>
        /// 0 = INSERT, > 0 = UPDATE (edit mode)
        /// </summary>
        public int OutwardId { get; set; } = 0;

        [Required(ErrorMessage = "CompanyId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CompanyId must be greater than 0")]
        public int CompanyId { get; set; }

        public int? StyleId { get; set; }

        public int? DesignId { get; set; }

        [Required(ErrorMessage = "Colour is required")]
        [StringLength(10000)]
        public string Colour { get; set; } = string.Empty;

        [Required(ErrorMessage = "DesignName is required")]
        [StringLength(10000)]
        public string DesignName { get; set; } = string.Empty;

        [Required(ErrorMessage = "StyleNo is required")]
        [StringLength(10000)]
        public string StyleNo { get; set; } = string.Empty;

        [StringLength(10000)]
        public string OutwardDcNo { get; set; } = string.Empty;

        public char EntryType { get; set; } = 'M';

        public string? DeliveryTo { get; set; }
        public string? PoNo { get; set; }
        public string? Weight { get; set; }
        public string? NoOfBundles { get; set; }

        /// <summary>
        /// "INSERT" or "UPDATE"
        /// </summary>
        [Required(ErrorMessage = "Mode is required")]
        public string Mode { get; set; } = "INSERT";

        [Required(ErrorMessage = "CreatedBy is required")]
        public string CreatedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = "MeterDetails list cannot be empty")]
        [MinLength(1, ErrorMessage = "At least one meter detail must be provided")]
        public List<OutwardMeterDetailDto> MeterDetails { get; set; } = new List<OutwardMeterDetailDto>();
    }
}
