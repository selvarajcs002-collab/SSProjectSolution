using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SSProjectSolution.Request
{
    public class InwardSaveRequest
    {
        [Required(ErrorMessage = "Inward details are required")]
        public InwardCreateDto Inward { get; set; } = new InwardCreateDto();

        [Required(ErrorMessage = "Sizes list cannot be empty")]
        [MinLength(1, ErrorMessage = "At least one size must be provided")]
        public List<SizeDto> Sizes { get; set; } = new List<SizeDto>();
    }
}
