public interface IWhatsAppService
{
    Task SendInwardMessageAsync(
        WhatsAppNotificationDto model);

    Task SendOutwardMessageAsync(
        WhatsAppNotificationDto model);
}