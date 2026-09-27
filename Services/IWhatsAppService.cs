using System.Threading.Tasks;
using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Request;

namespace SSProjectSolution.Services
{
    public interface IWhatsAppService
    {
        Task SendInwardMessageAsync(WhatsAppNotificationDto model);
        Task SendOutwardMessageAsync(WhatsAppNotificationDto model);
        
        Task<WhatsAppResponse> SendInwardAsync(InwardWhatsAppRequest request);
        Task<WhatsAppResponse> SendOutwardAsync(OutwardWhatsAppRequest request);
    }
}