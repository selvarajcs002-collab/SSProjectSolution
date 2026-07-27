using System.Collections.Generic;

namespace SSProjectSolution.Response
{
    public class InwardMatrixResponseDto
    {
        public List<string> Colours { get; set; } = new List<string>();
        public List<string> Sizes { get; set; } = new List<string>();
        public List<Request.MatrixItemDto> Matrix { get; set; } = new List<Request.MatrixItemDto>();
    }
}
