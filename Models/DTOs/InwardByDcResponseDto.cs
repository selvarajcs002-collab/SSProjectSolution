using System.Text.Json.Serialization;

namespace SSProjectSolution.Models.DTOs
{
    public class InwardByDcResponseDto
    {
        [JsonPropertyName("inward_id")]
        public int InwardId { get; set; }

        [JsonPropertyName("company_id")]
        public int CompanyId { get; set; }

        [JsonPropertyName("colour")]
        public string Colour { get; set; }

        [JsonPropertyName("design_name")]
        public string DesignName { get; set; }

        [JsonPropertyName("style_no")]
        public string StyleNo { get; set; }

        [JsonPropertyName("inward_dc_no")]
        public string InwardDcNo { get; set; }
    }
}
