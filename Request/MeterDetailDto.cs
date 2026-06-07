using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SSProjectSolution.Request
{
    public class MeterDetailDto
    {
        [Required(ErrorMessage = "MeterValue is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "MeterValue must be greater than 0")]
        public decimal MeterValue { get; set; }

        // Fallback for older cached frontend payloads that still send meterPerBit
        [JsonPropertyName("meterPerBit")]
        public decimal MeterPerBit
        {
            get => MeterValue;
            set => MeterValue = value;
        }

        [Required(ErrorMessage = "BitsCount is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "BitsCount must be greater than 0")]
        public decimal BitsCount { get; set; }
        
        // Frontend total meter is just for reference in the request, backend will recalculate it.
        public decimal? TotalMeter { get; set; }

        public decimal PiecesCount { get; set; }
    }
}
