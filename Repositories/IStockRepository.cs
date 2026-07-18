using System.Collections.Generic;
using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Repositories
{
    public interface IStockRepository
    {
        Task<StockSummaryDto> GetStockSummaryAsync(StockFilterRequest filter);
        Task<IEnumerable<StockBalanceDto>> GetStockBalanceAsync(StockFilterRequest filter);
        Task<IEnumerable<StockTransactionDto>> GetLastTransactionsAsync(StockFilterRequest filter);
        Task<IEnumerable<DeliveryChallanDto>> GetDeliveryChallansAsync(int? companyId, string? styleNo, string? designName, string? colour, System.Threading.CancellationToken cancellationToken = default);
        Task<IEnumerable<FilterOptionDto>> GetStylesAsync(int companyId, System.Threading.CancellationToken cancellationToken = default);
        Task<IEnumerable<FilterOptionDto>> GetDesignsAsync(int companyId, string styleNo, System.Threading.CancellationToken cancellationToken = default);
        Task<IEnumerable<FilterOptionDto>> GetColoursAsync(int companyId, string styleNo, string designName, System.Threading.CancellationToken cancellationToken = default);
    }
}
