using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SSProjectSolution.Request
{
    public class InwardMultipleColourSaveRequest
    {
        [Required(ErrorMessage = "Inward details are required")]
        public InwardHeaderDto Inward { get; set; } = new InwardHeaderDto();

        [Required(ErrorMessage = "Colours list cannot be empty")]
        [MinLength(1, ErrorMessage = "At least one colour must be provided")]
        public List<string> Colours { get; set; } = new List<string>();

        [Required(ErrorMessage = "Sizes list cannot be empty")]
        [MinLength(1, ErrorMessage = "At least one size must be provided")]
        public List<string> Sizes { get; set; } = new List<string>();

        [Required(ErrorMessage = "Matrix cannot be empty")]
        [MinLength(1, ErrorMessage = "Matrix data must be provided")]
        public List<MatrixItemDto> Matrix { get; set; } = new List<MatrixItemDto>();
    }
}
