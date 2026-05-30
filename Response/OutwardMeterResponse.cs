using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SSProjectSolution.Response
{
    public class OutwardMeterResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("outwardId")]
        public int OutwardId { get; set; }

        [JsonPropertyName("outwardDcNo")]
        public string OutwardDcNo { get; set; } = string.Empty;
    }

    public class OutwardMeterByDcResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("companyId")]
        public int? CompanyId { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("colour")]
        public string? Colour { get; set; }

        [JsonPropertyName("designName")]
        public string? DesignName { get; set; }

        [JsonPropertyName("styleNo")]
        public string? StyleNo { get; set; }

        [JsonPropertyName("dcNo")]
        public string? DcNo { get; set; }

        [JsonPropertyName("entryType")]
        public string? EntryType { get; set; }

        [JsonPropertyName("createdBy")]
        public string? CreatedBy { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("meterDetails")]
        public List<OutwardMeterDetailResponseDto> MeterDetails { get; set; } = new();
    }

    public class OutwardMeterDetailResponseDto
    {
        [JsonPropertyName("omdId")]
        public long OmdId { get; set; }

        [JsonPropertyName("meterValue")]
        public decimal MeterValue { get; set; }

        [JsonPropertyName("bitsCount")]
        public decimal BitsCount { get; set; }

        [JsonPropertyName("totalMeter")]
        public decimal TotalMeter { get; set; }
    }
}
