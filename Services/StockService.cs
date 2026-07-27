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

        public async Task<IEnumerable<DeliveryChallanDto>> GetDeliveryChallansAsync(int? companyId, string? styleNo, string? designName, string? colour, System.Threading.CancellationToken cancellationToken = default)
        {
            return await _stockRepository.GetDeliveryChallansAsync(companyId, styleNo, designName, colour, cancellationToken);
        }

        public async Task<IEnumerable<FilterOptionDto>> GetStylesAsync(int companyId, System.Threading.CancellationToken cancellationToken = default)
        {
            return await _stockRepository.GetStylesAsync(companyId, cancellationToken);
        }

        public async Task<IEnumerable<FilterOptionDto>> GetDesignsAsync(int companyId, string styleNo, System.Threading.CancellationToken cancellationToken = default)
        {
            return await _stockRepository.GetDesignsAsync(companyId, styleNo, cancellationToken);
        }

        public async Task<IEnumerable<FilterOptionDto>> GetColoursAsync(int companyId, string styleNo, string designName, System.Threading.CancellationToken cancellationToken = default)
        {
            return await _stockRepository.GetColoursAsync(companyId, styleNo, designName, cancellationToken);
        }
    }
}
