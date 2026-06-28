namespace SSProjectSolution.Response
{
    public class PrintChallanResponse
    {
        public bool PdfGenerated { get; set; }
        public bool PdfSaved { get; set; }
        public bool PrintSuccess { get; set; }
        public string SavedFilePath { get; set; }
        public string PrinterName { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}
