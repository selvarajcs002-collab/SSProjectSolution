using System.Collections.Generic;
using System.Threading.Tasks;
using SSProjectSolution.Repositories;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;

        public StockService(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<StockSummaryDto> GetStockSummaryAsync(StockFilterRequest filter)
        {
            return await _stockRepository.GetStockSummaryAsync(filter);
        }

        public async Task<IEnumerable<StockBalanceDto>> GetStockBalanceAsync(StockFilterRequest filter)
        {
            return await _stockRepository.GetStockBalanceAsync(filter);
        }

        public async Task<IEnumerable<StockTransactionDto>> GetLastTransactionsAsync(StockFilterRequest filter)
        {
            return await _stockRepository.GetLastTransactionsAsync(filter);
        }
    }
}
