using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SSProjectSolution.Services;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockManagementController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockManagementController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpPost("summary")]
        public async Task<IActionResult> GetStockSummary([FromBody] StockFilterRequest filter)
        {
            try
            {
                var summary = await _stockService.GetStockSummaryAsync(filter);
                return Ok(new ApiResponse<StockSummaryDto> { Success = true, Data = summary, Message = "Success" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("balance")]
        public async Task<IActionResult> GetStockBalance([FromBody] StockFilterRequest filter)
        {
            try
            {
                var balance = await _stockService.GetStockBalanceAsync(filter);
                return Ok(new ApiResponse<System.Collections.Generic.IEnumerable<StockBalanceDto>> { Success = true, Data = balance, Message = "Success" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> GetLastTransactions([FromBody] StockFilterRequest filter)
        {
            try
            {
                var transactions = await _stockService.GetLastTransactionsAsync(filter);
                return Ok(new ApiResponse<System.Collections.Generic.IEnumerable<StockTransactionDto>> { Success = true, Data = transactions, Message = "Success" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }
    }
}
