using System.Collections.Generic;

namespace SSProjectSolution.Request
{
    public class OutwardRequest
    {
        public OutwardData Outward { get; set; }
        public List<SizeData> Sizes { get; set; }
    }

    public class OutwardData
    {
        public int OutwardId { get; set; }
        public string Mode { get; set; }
        public int CompanyId { get; set; }
        public string Colour { get; set; }
        public string DesignName { get; set; }
        public string StyleNo { get; set; }
        public string UploadURL { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
    }

    public class SizeData
    {
        public string Size { get; set; }
        public int Count { get; set; }
    }
}
