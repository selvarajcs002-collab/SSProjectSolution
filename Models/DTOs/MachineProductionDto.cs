using System;

namespace SSProjectSolution.Models.DTOs
{
    public class MachineProductionDto
    {
        public int? Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public string StyleName { get; set; } = string.Empty;
        public string DesignName { get; set; } = string.Empty;
        public int TotalProduction { get; set; }
        public int TargetProduction { get; set; }
        public decimal CostPerPiece { get; set; }
        public decimal ProductionCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
