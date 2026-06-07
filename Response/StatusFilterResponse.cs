using System.Collections.Generic;

namespace SSProjectSolution.Response
{
    public class StatusFilterResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (TotalRecords + PageSize - 1) / PageSize : 0;
        public object Summary { get; set; } = new { };
        public List<dynamic> Data { get; set; } = new List<dynamic>();
    }
}
