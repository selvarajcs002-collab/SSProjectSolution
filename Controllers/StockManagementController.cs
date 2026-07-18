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
        [HttpGet("deliverychallans")]
        public async Task<IActionResult> GetDeliveryChallans([FromQuery] int? companyId, [FromQuery] string? styleNo, [FromQuery] string? designNo, [FromQuery] string? colour, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                if (companyId == null || string.IsNullOrWhiteSpace(styleNo) || string.IsNullOrWhiteSpace(designNo) || string.IsNullOrWhiteSpace(colour))
                {
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Company, Style, Design, and Colour are required." });
                }

                var deliveryChallans = await _stockService.GetDeliveryChallansAsync(companyId, styleNo, designNo, colour, cancellationToken);
                return Ok(new ApiResponse<System.Collections.Generic.IEnumerable<DeliveryChallanDto>> { Success = true, Data = deliveryChallans, Message = "Success" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("styles")]
        public async Task<IActionResult> GetStyles([FromQuery] int? companyId, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                if (companyId == null)
                {
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Company is required." });
                }

                var styles = await _stockService.GetStylesAsync(companyId.Value, cancellationToken);
                return Ok(new ApiResponse<System.Collections.Generic.IEnumerable<FilterOptionDto>> { Success = true, Data = styles, Message = "Success" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("designs")]
        public async Task<IActionResult> GetDesigns([FromQuery] int? companyId, [FromQuery] string? styleNo, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                if (companyId == null || string.IsNullOrWhiteSpace(styleNo))
                {
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Company and Style are required." });
                }

                var designs = await _stockService.GetDesignsAsync(companyId.Value, styleNo, cancellationToken);
                return Ok(new ApiResponse<System.Collections.Generic.IEnumerable<FilterOptionDto>> { Success = true, Data = designs, Message = "Success" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("colours")]
        public async Task<IActionResult> GetColours([FromQuery] int? companyId, [FromQuery] string? styleNo, [FromQuery] string? designNo, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                if (companyId == null || string.IsNullOrWhiteSpace(styleNo) || string.IsNullOrWhiteSpace(designNo))
                {
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Company, Style, and Design are required." });
                }

                var colours = await _stockService.GetColoursAsync(companyId.Value, styleNo, designNo, cancellationToken);
                return Ok(new ApiResponse<System.Collections.Generic.IEnumerable<FilterOptionDto>> { Success = true, Data = colours, Message = "Success" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }
    }
}
