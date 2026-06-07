using Newtonsoft.Json;

namespace SSProjectSolution.Models.DTOs
{
    public class InwardUpdateDto
    {
        [JsonProperty("inward_id")]
        public int InwardId { get; set; }

        [JsonProperty("company_id")]
        public int CompanyId { get; set; }

        [JsonProperty("colour")]
        public string Colour { get; set; }

        [JsonProperty("design_name")]
        public string DesignName { get; set; }

        [JsonProperty("style_no")]
        public string StyleNo { get; set; }

        [JsonProperty("inward_dc_no")]
        public string InwardDcNo { get; set; }

        [JsonProperty("updated_by")]
        public int UpdatedBy { get; set; }

        [JsonProperty("entry_type")]
        public char EntryType { get; set; } = 'S';

        [JsonProperty("sizes")]
        public List<SSProjectSolution.Request.SizeDto> Sizes { get; set; } = new List<SSProjectSolution.Request.SizeDto>();

        [JsonProperty("meter_details")]
        public List<SSProjectSolution.Request.MeterDetailDto> MeterDetails { get; set; } = new List<SSProjectSolution.Request.MeterDetailDto>();
    }
}
