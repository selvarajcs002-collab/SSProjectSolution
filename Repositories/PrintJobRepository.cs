using Dapper;
using Microsoft.Extensions.Logging;
using SSProjectSolution.Data;
using SSProjectSolution.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SSProjectSolution.Repositories
{
    public class PrintJobRepository : IPrintJobRepository
    {
        private readonly DapperDBConnection _dbConnection;
        private readonly ILogger<PrintJobRepository> _logger;

        public PrintJobRepository(DapperDBConnection dbConnection, ILogger<PrintJobRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<string> CreateJobAsync(PrintJob job)
        {
            using var connection = _dbConnection.CreateConnection();
            var sql = @"
                INSERT INTO PrintJobs (
                    JobId, DocumentType, DocumentNumber, PdfPath, PrinterName, Copies, 
                    PaperSize, Orientation, Status, RetryCount, UserId, CompanyId, 
                    CreatedDate, Downloaded, Printed
                ) VALUES (
                    @JobId, @DocumentType, @DocumentNumber, @PdfPath, @PrinterName, @Copies, 
                    @PaperSize, @Orientation, @Status, @RetryCount, @UserId, @CompanyId, 
                    @CreatedDate, @Downloaded, @Printed
                )";

            await connection.ExecuteAsync(sql, job);
            return job.JobId;
        }

        public async Task<PrintJob?> GetJobByIdAsync(string jobId)
        {
            using var connection = _dbConnection.CreateConnection();
            var sql = "SELECT * FROM PrintJobs WHERE JobId = @JobId";
            return await connection.QuerySingleOrDefaultAsync<PrintJob>(sql, new { JobId = jobId });
        }

        public async Task<IEnumerable<PrintJob>> GetPendingJobsByUserIdAsync(string userId)
        {
            using var connection = _dbConnection.CreateConnection();
            var sql = "SELECT * FROM PrintJobs WHERE UserId = @UserId AND Status IN ('Queued', 'Sent') ORDER BY CreatedDate ASC";
            return await connection.QueryAsync<PrintJob>(sql, new { UserId = userId });
        }

        public async Task<bool> UpdateJobStatusAsync(string jobId, string status, string? failureReason = null)
        {
            using var connection = _dbConnection.CreateConnection();
            var sql = @"
                UPDATE PrintJobs 
                SET Status = @Status, 
                    FailureReason = @FailureReason,
                    CompletedDate = CASE WHEN @Status IN ('Printed', 'Failed') THEN GETDATE() ELSE CompletedDate END,
                    Printed = CASE WHEN @Status = 'Printed' THEN 1 ELSE Printed END
                WHERE JobId = @JobId";
            
            var rowsAffected = await connection.ExecuteAsync(sql, new { JobId = jobId, Status = status, FailureReason = failureReason });
            return rowsAffected > 0;
        }

        public async Task<bool> MarkJobAsDownloadedAsync(string jobId)
        {
            using var connection = _dbConnection.CreateConnection();
            var sql = "UPDATE PrintJobs SET Downloaded = 1 WHERE JobId = @JobId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { JobId = jobId });
            return rowsAffected > 0;
        }

        public async Task<bool> IncrementRetryCountAsync(string jobId)
        {
            using var connection = _dbConnection.CreateConnection();
            var sql = "UPDATE PrintJobs SET RetryCount = RetryCount + 1 WHERE JobId = @JobId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { JobId = jobId });
            return rowsAffected > 0;
        }
    }
}
