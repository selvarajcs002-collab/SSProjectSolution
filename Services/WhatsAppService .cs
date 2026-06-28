using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppSettings _settings;

    public WhatsAppService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        _settings = configuration
            .GetSection("WhatsAppSettings")
            .Get<WhatsAppSettings>();
    }

    public async Task SendInwardMessageAsync(
        WhatsAppNotificationDto model)
    {
        var message = BuildMessage(model);

        await SendMessageAsync(message);
    }

    public async Task SendOutwardMessageAsync(
        WhatsAppNotificationDto model)
    {
        var message = BuildMessage(model);

        await SendMessageAsync(message);
    }

    private async Task SendMessageAsync(
        string message)
    {
        var url =
            $"https://graph.facebook.com/v20.0/{_settings.PhoneNumberId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = _settings.RecipientNumber,
            type = "text",
            text = new
            {
                body = message
            }
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _settings.AccessToken);

        request.Content =
            new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

        var response =
            await _httpClient.SendAsync(request);

        var responseText =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(responseText);
        }
    }

    private string BuildMessage(
        WhatsAppNotificationDto model)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            model.EntryType == "INWARD"
            ? "📥 INWARD ENTRY"
            : "📤 OUTWARD ENTRY");

        sb.AppendLine();

        sb.AppendLine($"🏢 Company : {model.CompanyName}");
        sb.AppendLine($"🧵 Style No : {model.StyleNo}");
        sb.AppendLine($"🎨 Design : {model.DesignName}");
        sb.AppendLine($"📦 Type : {model.Mode}");

        if (!string.IsNullOrWhiteSpace(model.DcNo))
        {
            sb.AppendLine($"📄 DC No : {model.DcNo}");
        }

        sb.AppendLine();
        sb.AppendLine("━━━━━━━━━━━━━━");
        sb.AppendLine();

        if (model.Mode == "SIZE")
        {
            sb.AppendLine("📏 SIZE DETAILS");
        }
        else
        {
            sb.AppendLine("📏 METER DETAILS");
        }

        sb.AppendLine();

        foreach (var item in model.Items)
        {
            sb.AppendLine($"{item.SizeName} : {item.Quantity}");
        }

        sb.AppendLine();
        sb.AppendLine("━━━━━━━━━━━━━━");
        sb.AppendLine();

        sb.AppendLine(
            $"📊 Total Qty : {model.TotalCount}");

        sb.AppendLine();

        sb.AppendLine(
            $"📅 {DateTime.Now:dd-MMM-yyyy}");

        sb.AppendLine(
            $"🕒 {DateTime.Now:hh:mm tt}");

        sb.AppendLine();

        sb.AppendLine("✅ Successfully Created");

        return sb.ToString();
    }
}