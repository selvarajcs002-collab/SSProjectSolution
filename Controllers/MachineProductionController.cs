using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;
using SSProjectSolution.Data;
using SSProjectSolution.Models.DTOs;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineProductionController : ControllerBase
    {
        private readonly DapperDBConnection _dbConnection;

        public MachineProductionController(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProduction([FromBody] MachineProductionDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { message = "Invalid production data" });

                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeName", model.EmployeeName);
                parameters.Add("@MachineName", model.MachineName);
                parameters.Add("@Shift", model.Shift);
                parameters.Add("@StyleName", model.StyleName);
                parameters.Add("@DesignName", model.DesignName);
                parameters.Add("@TotalProduction", model.TotalProduction);
                parameters.Add("@TargetProduction", model.TargetProduction);
                parameters.Add("@CostPerPiece", model.CostPerPiece);
                parameters.Add("@ProductionCost", model.ProductionCost);
                parameters.Add("@Status", model.Status);
                parameters.Add("@CompanyId", model.CompanyId);

                var result = await connection.QueryAsync<int>(
                    "sp_EMP_AddDailyProduction",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var newId = result.FirstOrDefault();
                return Ok(new { success = true, id = newId, message = "Production entry saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("list/{companyId}")]
        public async Task<IActionResult> GetProductionList(int companyId, [FromQuery] string? shift = null)
        {
            try
            {
                if (companyId <= 0)
                    return BadRequest(new { message = "Invalid CompanyId" });

                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@CompanyId", companyId);
                parameters.Add("@Shift", shift);

                var records = await connection.QueryAsync<MachineProductionDto>(
                    "sp_EMP_GetDailyProductionsByCompany",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("paginated-list")]
        public async Task<IActionResult> GetPaginatedProductionList([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? shift = null)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Shift", shift);

                // Assuming sp_EMP_GetAllDailyProductions is created to fetch all records
                var records = await connection.QueryAsync<MachineProductionDto>(
                    "sp_EMP_GetAllDailyProductions",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // Order by latest entry first (using Id descending) and paginate
                var paginatedRecords = records
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalRecords = records.Count();

                return Ok(new
                {
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    CurrentPage = page,
                    PageSize = pageSize,
                    Data = paginatedRecords
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
