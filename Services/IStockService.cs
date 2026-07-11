using System.Collections.Generic;
using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IStockService
    {
        Task<StockSummaryDto> GetStockSummaryAsync(StockFilterRequest filter);
        Task<IEnumerable<StockBalanceDto>> GetStockBalanceAsync(StockFilterRequest filter);
        Task<IEnumerable<StockTransactionDto>> GetLastTransactionsAsync(StockFilterRequest filter);
    }
}
