using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SSProjectSolution.Request
{
    public class OutwardMeterDetailDto
    {
        [Required(ErrorMessage = "MeterValue is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "MeterValue must be greater than 0")]
        public decimal MeterValue { get; set; }

        // Fallback binding for any legacy payload that still sends meterPerBit
        [JsonPropertyName("meterPerBit")]
        public decimal MeterPerBit
        {
            get => MeterValue;
            set => MeterValue = value;
        }

        [Required(ErrorMessage = "BitsCount is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "BitsCount must be greater than 0")]
        public decimal BitsCount { get; set; }

        /// <summary>
        /// Frontend-supplied hint. Backend ALWAYS recalculates (MeterValue × BitsCount).
        /// </summary>
        public decimal TotalMeter { get; set; }

        public decimal PiecesCount { get; set; }
    }
}
