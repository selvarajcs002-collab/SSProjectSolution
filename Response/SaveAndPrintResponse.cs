using System;

namespace SSProjectSolution.Response
{
    public class SaveAndPrintResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SavedFilePath { get; set; } = string.Empty;
        public bool Printed { get; set; }
        public int Copies { get; set; }
        public string Printer { get; set; } = string.Empty;
        public long ExecutionTimeMs { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
    }
}
