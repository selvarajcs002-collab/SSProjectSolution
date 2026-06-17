namespace SSProjectSolution.Models.DTOs
{
    public class SizeResponseDto
    {
        public string Size { get; set; }

        /// <summary>Total inward quantity for this Company + StyleNo + Colour + Size.</summary>
        public int Count { get; set; }

        /// <summary>
        /// Net available quantity = TotalInward - TotalOutward.
        /// Guaranteed to be >= 0 (clamped at the SQL layer).
        /// </summary>
        public int AvailableQty { get; set; }
    }
}
