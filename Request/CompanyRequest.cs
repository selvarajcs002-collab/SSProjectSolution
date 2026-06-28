namespace SSProjectSolution.Request
{
    public class CompanyRequest
    {
        public string Mode { get; set; } = string.Empty; // 'INSERT' or 'UPDATE'
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Gst_No { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Door_No { get; set; } = string.Empty;
        public string Street_Name { get; set; } = string.Empty;
        public string Landmark { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public List<string> DeliveryToLocations { get; set; } = new List<string>();
    }
}
