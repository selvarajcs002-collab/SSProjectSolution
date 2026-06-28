using Newtonsoft.Json.Linq;
using SSProjectSolution.Response;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public interface IDeliveryChallanService
    {
        Task<SaveAndPrintResponse> ProcessSaveAndPrintAsync(JObject payload);
    }
}
