using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Data;
using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Request;
using SSProjectSolution.Data;

namespace SSProjectSolution.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly WhatsAppSettings _settings;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly DapperDBConnection _dbConnection;

        public WhatsAppService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<WhatsAppService> logger,
            DapperDBConnection dbConnection)
        {
            _httpClient = httpClient;
            _logger = logger;
            _dbConnection = dbConnection;
            _settings = configuration.GetSection("WhatsAppSettings").Get<WhatsAppSettings>() ?? new WhatsAppSettings();

            if (_settings.TimeoutSeconds > 0)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
            }
        }

        // Legacy methods
        public async Task SendInwardMessageAsync(WhatsAppNotificationDto model)
        {
            var message = BuildLegacyMessage(model);
            await SendLegacyMessageAsync(message);
        }

        public async Task SendOutwardMessageAsync(WhatsAppNotificationDto model)
        {
            var message = BuildLegacyMessage(model);
            await SendLegacyMessageAsync(message);
        }

        // New GROUP methods
        public async Task<WhatsAppResponse> SendInwardAsync(InwardWhatsAppRequest request)
        {
            if (!_settings.Enabled) return new WhatsAppResponse { Success = true, Message = "WhatsApp integration is disabled." };

            try
            {
                if (await IsMessageSentAsync("INWARD", request.InwardId))
                {
                    return new WhatsAppResponse { Success = true, Message = "Message already sent." };
                }

                var text = WhatsAppServiceBuilder.BuildInwardMessage(request);
                await SendGroupMessageAsync(text);
                await LogMessageAsync("INWARD", request.InwardId, _settings.GroupId, text, "SENT", null, null);
                
                return new WhatsAppResponse { Success = true, Message = "Message sent successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp Inward notification for InwardId: {InwardId}", request.InwardId);
                await LogMessageAsync("INWARD", request.InwardId, _settings.GroupId, "Error", "FAILED", null, ex.Message);
                return new WhatsAppResponse { Success = false, Message = "Error sending message: " + ex.Message };
            }
        }

        public async Task<WhatsAppResponse> SendOutwardAsync(OutwardWhatsAppRequest request)
        {
            if (!_settings.Enabled) return new WhatsAppResponse { Success = true, Message = "WhatsApp integration is disabled." };

            try
            {
                if (await IsMessageSentAsync("OUTWARD", request.OutwardId))
                {
                    return new WhatsAppResponse { Success = true, Message = "Message already sent." };
                }

                var text = WhatsAppServiceBuilder.BuildOutwardMessage(request);
                await SendGroupMessageAsync(text);
                await LogMessageAsync("OUTWARD", request.OutwardId, _settings.GroupId, text, "SENT", null, null);
                
                return new WhatsAppResponse { Success = true, Message = "Message sent successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp Outward notification for OutwardId: {OutwardId}", request.OutwardId);
                await LogMessageAsync("OUTWARD", request.OutwardId, _settings.GroupId, "Error", "FAILED", null, ex.Message);
                return new WhatsAppResponse { Success = false, Message = "Error sending message: " + ex.Message };
            }
        }

        private async Task SendGroupMessageAsync(string text)
        {
            if (string.IsNullOrEmpty(_settings.Provider))
                throw new Exception("Provider is not configured");

            if (_settings.Provider.Equals("Gupshup", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new
                {
                    channel = "whatsapp",
                    source = "your_gupshup_number",
                    destination = _settings.GroupId,
                    message = JsonSerializer.Serialize(new { type = "text", text = text }),
                    src_name = "SSManagement"
                };

                var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl);
                request.Headers.Add("apikey", _settings.ApiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Gupshup API Error: {response.StatusCode} - {responseText}");
            }
            else
            {
                // Generic JSON Provider
                var payload = new
                {
                    groupId = _settings.GroupId,
                    message = text
                };

                var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl);
                if (!string.IsNullOrEmpty(_settings.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
                }
                
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Provider API Error: {response.StatusCode} - {responseText}");
            }
        }

        private async Task<bool> IsMessageSentAsync(string operationType, int referenceId)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();
                var status = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT Status FROM WhatsAppMessageLog WHERE OperationType = @OperationType AND ReferenceId = @ReferenceId",
                    new { OperationType = operationType, ReferenceId = referenceId });
                
                return status == "SENT";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not check WhatsAppMessageLog");
                return false;
            }
        }

        private async Task LogMessageAsync(string operationType, int referenceId, string groupId, string text, string status, string providerId, string error)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();
                var existingId = await connection.QueryFirstOrDefaultAsync<int?>(
                    "SELECT Id FROM WhatsAppMessageLog WHERE OperationType = @OperationType AND ReferenceId = @ReferenceId",
                    new { OperationType = operationType, ReferenceId = referenceId });

                if (existingId.HasValue)
                {
                    await connection.ExecuteAsync(
                        "UPDATE WhatsAppMessageLog SET Status = @Status, ErrorMessage = @Error, SentDate = @SentDate WHERE Id = @Id",
                        new { Id = existingId.Value, Status = status, Error = error, SentDate = status == "SENT" ? (DateTime?)DateTime.Now : null });
                }
                else
                {
                    await connection.ExecuteAsync(
                        @"INSERT INTO WhatsAppMessageLog (OperationType, ReferenceId, GroupId, MessageText, ProviderMessageId, Status, ErrorMessage, SentDate)
                          VALUES (@OperationType, @ReferenceId, @GroupId, @MessageText, @ProviderMessageId, @Status, @ErrorMessage, @SentDate)",
                        new
                        {
                            OperationType = operationType,
                            ReferenceId = referenceId,
                            GroupId = groupId ?? "",
                            MessageText = text ?? "",
                            ProviderMessageId = providerId,
                            Status = status,
                            ErrorMessage = error,
                            SentDate = status == "SENT" ? (DateTime?)DateTime.Now : null
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not insert/update WhatsAppMessageLog");
            }
        }

        private async Task SendLegacyMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(_settings.PhoneNumberId) || string.IsNullOrEmpty(_settings.AccessToken))
                return;

            var url = $"https://graph.facebook.com/v20.0/{_settings.PhoneNumberId}/messages";
            var payload = new
            {
                messaging_product = "whatsapp",
                to = _settings.RecipientNumber,
                type = "text",
                text = new { body = message }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(responseText);
            }
        }

        private string BuildLegacyMessage(WhatsAppNotificationDto model)
        {
            var sb = new StringBuilder();
            sb.AppendLine(model.EntryType == "INWARD" ? "📥 INWARD ENTRY" : "📤 OUTWARD ENTRY");
            sb.AppendLine();
            sb.AppendLine($"🏢 Company : {model.CompanyName}");
            sb.AppendLine($"🧵 Style No : {model.StyleNo}");
            sb.AppendLine($"🎨 Design : {model.DesignName}");
            sb.AppendLine($"📦 Type : {model.Mode}");

            if (!string.IsNullOrWhiteSpace(model.DcNo))
                sb.AppendLine($"📄 DC No : {model.DcNo}");

            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━");
            sb.AppendLine();

            if (model.Mode == "SIZE")
                sb.AppendLine("📏 SIZE DETAILS");
            else
                sb.AppendLine("📏 METER DETAILS");

            sb.AppendLine();

            foreach (var item in model.Items)
                sb.AppendLine($"{item.SizeName} : {item.Quantity}");

            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine($"📊 Total Qty : {model.TotalCount}");
            sb.AppendLine();
            sb.AppendLine($"📅 {DateTime.Now:dd-MMM-yyyy}");
            sb.AppendLine($"🕒 {DateTime.Now:hh:mm tt}");
            sb.AppendLine();
            sb.AppendLine("✅ Successfully Created");

            return sb.ToString();
        }
    }
}