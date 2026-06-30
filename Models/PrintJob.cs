using System;

namespace SSProjectSolution.Models
{
    public class PrintJob
    {
        public string JobId { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string PdfPath { get; set; } = string.Empty;
        public string? PrinterName { get; set; }
        public int Copies { get; set; } = 1;
        public string? PaperSize { get; set; }
        public string? Orientation { get; set; }
        
        // Status: "Queued", "Sent", "Printed", "Failed"
        public string Status { get; set; } = "Queued";
        
        public int RetryCount { get; set; } = 0;
        public string UserId { get; set; } = string.Empty;
        public int? CompanyId { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }
        public string? FailureReason { get; set; }
        
        public bool Downloaded { get; set; } = false;
        public bool Printed { get; set; } = false;
    }
}
