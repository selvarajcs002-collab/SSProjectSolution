using System.Threading.Tasks;
using SSProjectSolution.Models;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IAdvanceChallanService
    {
        Task<(string ChallanNo, string ChallanFileName)> GenerateAndSaveChallanAsync(AdvanceAmount advance);
        Task<AdvanceChallanFileResult> GenerateChallanAsync(GenerateAdvanceChallanRequest request, string generatedBy);
    }
}
