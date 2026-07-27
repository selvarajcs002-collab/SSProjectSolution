using System;

namespace SSProjectSolution.Response
{
    public class DeliveryChallanDto
    {
        public string DeliveryChallanNo { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
