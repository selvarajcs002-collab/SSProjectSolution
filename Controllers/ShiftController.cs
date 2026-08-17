using Microsoft.AspNetCore.Mvc;
using Dapper;
using SSProjectSolution.Data;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftController : ControllerBase
    {
        private readonly DapperDBConnection _dbConnection;

        public ShiftController(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        [HttpGet("machines")]
        public async Task<IActionResult> GetMachines()
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();
                string sql = "SELECT MachineId, MachineName, Head, IsActive FROM [dbo].[MachineMaster] WHERE [IsActive] = 1 ORDER BY [MachineName]";
                var machines = await connection.QueryAsync<dynamic>(sql);
                return Ok(machines);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("machines")]
        public async Task<IActionResult> CreateMachine([FromBody] object request)
        {
            try
            {
                using var httpClient = new HttpClient();
                var jsonString = JsonConvert.SerializeObject(request);
                var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync("http://localhost:5147/api/v1/machines", httpContent);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Content(content, "application/json");
                }
                
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
