namespace SSProjectSolution.Request
{
    public class ReuseDcNoRequest
    {
        public int CompanyId { get; set; }
        public string DcNo { get; set; } = string.Empty;
        public string ReuseReason { get; set; } = string.Empty;
        public string? ReusedBy { get; set; }
        public string? UserRole { get; set; }
    }

    public class DeleteOutwardRequest
    {
        public int OutwardId { get; set; }
        public string? DeletedBy { get; set; }
        public string? DeletionReason { get; set; }
    }
}
