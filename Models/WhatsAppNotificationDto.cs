public class WhatsAppNotificationDto
{
    public string CompanyName { get; set; }

    public string StyleNo { get; set; }

    public string DesignName { get; set; }

    public string EntryType { get; set; }

    public string Mode { get; set; }

    public int CreatedBy { get; set; }

    public string DcNo { get; set; }

    public int TotalCount { get; set; }

    public List<WhatsAppSizeDto> Items { get; set; }
}