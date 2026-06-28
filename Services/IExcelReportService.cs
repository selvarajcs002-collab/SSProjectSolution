using SSProjectSolution.Models.DTOs;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public interface IExcelReportService
    {
        Task<ReportResponseDto> GetDeliveryChallanReportAsync(ReportFilterRequestDto request);
    }
}
