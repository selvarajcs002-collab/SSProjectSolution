namespace SSProjectSolution.Response
{
    public class AdvanceChallanFileResult
    {
        public byte[] PdfBytes { get; set; } = System.Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ChallanNo { get; set; } = string.Empty;
    }
}
