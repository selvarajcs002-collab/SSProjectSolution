using System;
using System.Collections.Generic;

namespace SSProjectSolution.Request
{
    public class LotCompletedDto
    {
        public int CompanyId { get; set; }
        public string StyleNo { get; set; }
        public string DesignName { get; set; }
        public string Colour { get; set; }
        public string PoNo { get; set; }
        public bool IsDeliveryChallan { get; set; }
        public List<string> SelectedDcNos { get; set; }
        public string EntryType { get; set; }
        
        public List<ConsumedSizeDto> ConsumedSizes { get; set; }
        public List<ConsumedMeterDto> ConsumedMeters { get; set; }
    }

    public class ConsumedSizeDto
    {
        public string Size { get; set; }
        public int ConsumedQty { get; set; }
    }

    public class ConsumedMeterDto
    {
        public decimal MeterPerBit { get; set; }
        public int BitsCount { get; set; }
        public int PiecesCount { get; set; }
    }
}
