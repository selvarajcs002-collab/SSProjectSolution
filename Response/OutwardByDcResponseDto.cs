using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SSProjectSolution.Request;

namespace SSProjectSolution.Response
{
    public class OutwardByDcResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("companyId")]
        public int? CompanyId { get; set; }

        [JsonPropertyName("colour")]
        public string? Colour { get; set; }

        [JsonPropertyName("designName")]
        public string? DesignName { get; set; }

        [JsonPropertyName("styleNo")]
        public string? StyleNo { get; set; }

        [JsonPropertyName("uploadURL")]
        public string? UploadURL { get; set; }

        [JsonPropertyName("createdBy")]
        public string? CreatedBy { get; set; }

        [JsonPropertyName("createdDate")]
        public DateTime? CreatedDate { get; set; }

        [JsonPropertyName("updatedDate")]
        public DateTime? UpdatedDate { get; set; }

        [JsonPropertyName("dcNo")]
        public string? DcNo { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("entryType")]
        public string? EntryType { get; set; }

        [JsonPropertyName("meterDetails")]
        public List<MeterDetailDto> MeterDetails { get; set; } = new();

        [JsonPropertyName("sizeCounts")]
        public List<SizeCountDetailsDto> SizeCounts { get; set; } = new();

        [JsonPropertyName("colourBreakdowns")]
        public List<ColourBreakdownResponseDto> ColourBreakdowns { get; set; } = new();
    }

    public class ColourBreakdownResponseDto
    {
        [JsonPropertyName("colour")]
        public string Colour { get; set; }

        [JsonPropertyName("sizes")]
        public List<SizeCountDetailsDto> Sizes { get; set; } = new();
    }

    public class SizeCountDetailsDto
    {
        [JsonPropertyName("sizeCountId")]
        public int? SizeCountId { get; set; }

        [JsonPropertyName("size")]
        public string? Size { get; set; }

        [JsonPropertyName("count")]
        public int? Count { get; set; }
    }
}
