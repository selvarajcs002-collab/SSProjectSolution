using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SSProjectSolution.Models;
using SSProjectSolution.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintAgentController : ControllerBase
    {
        private readonly IPrintJobRepository _printJobRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PrintAgentController> _logger;

        public PrintAgentController(
            IPrintJobRepository printJobRepo,
            IConfiguration configuration,
            ILogger<PrintAgentController> logger)
        {
            _printJobRepo = printJobRepo;
            _configuration = configuration;
            _logger = logger;
        }

        public class AgentLoginRequest
        {
            public string AgentId { get; set; } = string.Empty;
            // Optionally, add a shared secret or password here for real security
            public string AgentSecret { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AgentLoginRequest request)
        {
            // In a real app, validate AgentId and AgentSecret against a database here.
            if (string.IsNullOrWhiteSpace(request.AgentId))
            {
                return BadRequest("AgentId is required.");
            }

            var jwtKey = _configuration["JwtSettings:SecretKey"];
            var jwtIssuer = _configuration["JwtSettings:Issuer"];
            var jwtAudience = _configuration["JwtSettings:Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, request.AgentId),
                    new Claim(JwtRegisteredClaimNames.Sub, request.AgentId)
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Long-lived token for the agent, or implement refresh tokens
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString, expires = tokenDescriptor.Expires });
        }

        [Authorize]
        [HttpGet("jobs/pending")]
        public async Task<IActionResult> GetPendingJobs()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var jobs = await _printJobRepo.GetPendingJobsByUserIdAsync(userId);
            return Ok(jobs);
        }

        [Authorize]
        [HttpGet("download/{jobId}")]
        public async Task<IActionResult> DownloadPdf(string jobId)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var job = await _printJobRepo.GetJobByIdAsync(jobId);
            if (job == null) return NotFound("Job not found.");

            // Security check: ensure the job belongs to the requesting user
            if (job.UserId != userId) return Forbid();

            if (!System.IO.File.Exists(job.PdfPath))
            {
                _logger.LogError("PDF not found at path: {PdfPath} for JobId: {JobId}", job.PdfPath, jobId);
                return NotFound("PDF file not found on server.");
            }

            // Mark as downloaded
            await _printJobRepo.MarkJobAsDownloadedAsync(jobId);

            var memory = new MemoryStream();
            using (var stream = new FileStream(job.PdfPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;



            return File(memory, "application/pdf", $"{job.DocumentNumber}.pdf");
        }

        public class JobStatusRequest
        {
            public string Status { get; set; } = string.Empty;
            public string? FailureReason { get; set; }
        }

        [Authorize]
        [HttpPost("jobs/{jobId}/status")]
        public async Task<IActionResult> UpdateJobStatus(string jobId, [FromBody] JobStatusRequest request)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var job = await _printJobRepo.GetJobByIdAsync(jobId);
            if (job == null) return NotFound("Job not found.");
            
            if (job.UserId != userId) return Forbid();

            bool updated = await _printJobRepo.UpdateJobStatusAsync(jobId, request.Status, request.FailureReason);

            if (request.Status == "Failed")
            {
                await _printJobRepo.IncrementRetryCountAsync(jobId);
            }
            else if (request.Status == "Printed" || request.Status == "Completed")
            {
                try
                {
                    if (System.IO.File.Exists(job.PdfPath))
                    {
                        System.IO.File.Delete(job.PdfPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temporary PDF file on server after print: {PdfPath}", job.PdfPath);
                }
            }

            return updated ? Ok() : StatusCode(500, "Failed to update job status.");
        }
    }
}
