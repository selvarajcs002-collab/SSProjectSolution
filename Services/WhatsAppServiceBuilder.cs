using System.Text;
using SSProjectSolution.Request;

namespace SSProjectSolution.Services
{
    public class WhatsAppServiceBuilder
    {
        public static string BuildInwardMessage(InwardWhatsAppRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("*SS MANAGEMENT - INWARD*");
            sb.AppendLine();
            sb.AppendLine("Inward completed successfully.");
            sb.AppendLine();
            sb.AppendLine($"Style No  : {request.StyleNo}");
            sb.AppendLine($"Design    : {request.DesignName}");
            sb.AppendLine($"Colour    : {request.Colour}");
            sb.AppendLine($"PO No     : {request.PoNo}");
            sb.AppendLine($"Inward DC : {request.InwardDcNo}");
            sb.AppendLine($"Quantity  : {request.Quantity}");
            sb.AppendLine($"Date      : {request.Date?.ToString("dd-MMM-yyyy")}");
            sb.AppendLine();
            sb.AppendLine("Status: INWARD COMPLETED");
            
            return sb.ToString();
        }

        public static string BuildOutwardMessage(OutwardWhatsAppRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("*SS MANAGEMENT - OUTWARD*");
            sb.AppendLine();
            sb.AppendLine("Outward completed successfully.");
            sb.AppendLine();
            sb.AppendLine($"Style No   : {request.StyleNo}");
            sb.AppendLine($"Design     : {request.DesignName}");
            sb.AppendLine($"Colour     : {request.Colour}");
            sb.AppendLine($"PO No      : {request.PoNo}");
            sb.AppendLine($"Outward DC : {request.OutwardDcNo}");
            sb.AppendLine($"Quantity   : {request.Quantity}");
            sb.AppendLine($"Date       : {request.Date?.ToString("dd-MMM-yyyy")}");
            sb.AppendLine();
            sb.AppendLine("Status: OUTWARD COMPLETED");
            
            return sb.ToString();
        }
    }
}
