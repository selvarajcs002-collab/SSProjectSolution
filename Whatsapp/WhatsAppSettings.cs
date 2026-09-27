public class WhatsAppSettings
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Gupshup";
    public string BaseUrl { get; set; }
    public string GroupId { get; set; }
    public string ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 10;
    
    public string AccessToken { get; set; }
    public string PhoneNumberId { get; set; }
    public string RecipientNumber { get; set; }
}