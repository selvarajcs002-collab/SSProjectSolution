using System.Collections.Generic;

namespace SSProjectSolution.Response
{
    public class StockManagementReportDto
    {
        public StockSummaryDto Summary { get; set; }
        public IEnumerable<StockBalanceDto> StockBalances { get; set; }
        public IEnumerable<StockTransactionDto> Transactions { get; set; }
        
        // Metadata for Header
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string CompanyName { get; set; }
        public string Branch { get; set; }
    }
}
