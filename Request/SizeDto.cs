using System.ComponentModel.DataAnnotations;

namespace SSProjectSolution.Request
{
    public class SizeDto
    {
        [Required(ErrorMessage = "Size is required")]
        [StringLength(10)]
        public string Size { get; set; } = string.Empty;

        [Required(ErrorMessage = "Count is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Count must be >= 0")]
        public int Count { get; set; }
    }
}
