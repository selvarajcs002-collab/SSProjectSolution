using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public interface IPdfGenerator
    {
        Task<byte[]> GeneratePdfAsync(JObject payload);
    }
}
