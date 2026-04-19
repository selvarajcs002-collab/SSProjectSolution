namespace SSProjectSolution.Request
{
    public class PrintPdfRequest
    {
        public string CompanyName { get; set; }
        public string DcNo { get; set; }
        public string Base64Pdf { get; set; }
    }
}
