using System.Collections.Generic;

namespace SSProjectSolution.Models.DTOs
{
    public class InwardEntryDetailsDto
    {
        public string EntryType { get; set; }
        public string Colour { get; set; }
        public List<SizeDetailDto> Sizes { get; set; } = new List<SizeDetailDto>();
        public List<MeterDetailDto> MeterDetails { get; set; } = new List<MeterDetailDto>();
    }

    public class SizeDetailDto
    {
        public string Size { get; set; }
        public int Count { get; set; }          // total inward count for this size
        public int AvailableQty { get; set; }   // inward count minus already-outward used qty
    }

    public class MeterDetailDto
    {
        public decimal MeterValue { get; set; }
        public decimal BitsCount { get; set; }
        public decimal TotalMeter { get; set; }
    }
}
