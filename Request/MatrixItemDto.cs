using System.ComponentModel.DataAnnotations;

namespace SSProjectSolution.Request
{
    public class MatrixItemDto
    {
        [Required(ErrorMessage = "Colour is required in matrix")]
        public string Colour { get; set; } = string.Empty;

        [Required(ErrorMessage = "Size is required in matrix")]
        public string Size { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Count must be >= 0")]
        public int? Count { get; set; }
    }
}
