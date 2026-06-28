using Dapper;
using Microsoft.Data.SqlClient;
using SSProjectSolution.Data;
using SSProjectSolution.Models;
using System.Data;

namespace SSProjectSolution.Repositories
{
    public class RateQuotationRepository : IRateQuotationRepository
    {
        private readonly DapperDBConnection _dbConnection;

        public RateQuotationRepository(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(long NewId, int StatusCode, string StatusMessage)> CreateAsync(RateQuotationEntity entity)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@QuotationDate", entity.QuotationDate);
            parameters.Add("@CompanyId", entity.CompanyId);
            parameters.Add("@CompanyName", entity.CompanyName);
            parameters.Add("@ContactPerson", entity.ContactPerson);
            parameters.Add("@MobileNo", entity.MobileNo);
            parameters.Add("@EmailId", entity.EmailId);
            parameters.Add("@Address", entity.Address);
            parameters.Add("@StyleNo", entity.StyleNo);
            parameters.Add("@DesignName", entity.DesignName);
            parameters.Add("@ProductType", entity.ProductType);
            parameters.Add("@RatePerPiece", entity.RatePerPiece);
            parameters.Add("@RatePerMeter", entity.RatePerMeter);
            parameters.Add("@NoOfStitches", entity.NoOfStitches);
            parameters.Add("@ChenilleColors", entity.ChenilleColors);
            parameters.Add("@NormalEmbColors", entity.NormalEmbColors);
            parameters.Add("@Quantity", entity.Quantity);
            parameters.Add("@TotalAmount", entity.TotalAmount);
            parameters.Add("@Remarks", entity.Remarks);
            parameters.Add("@Status", entity.Status);
            parameters.Add("@CreatedBy", entity.CreatedBy);
            
            parameters.Add("@NewId", dbType: DbType.Int64, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@StatusMessage", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("USP_RateQuotation_Insert", parameters, commandType: CommandType.StoredProcedure);

            var newId = parameters.Get<long>("@NewId");
            var statusCode = parameters.Get<int>("@StatusCode");
            var statusMessage = parameters.Get<string>("@StatusMessage");

            return (newId, statusCode, statusMessage);
        }

        public async Task<(int StatusCode, string StatusMessage)> UpdateAsync(RateQuotationEntity entity)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", entity.Id);
            parameters.Add("@QuotationDate", entity.QuotationDate);
            parameters.Add("@CompanyId", entity.CompanyId);
            parameters.Add("@CompanyName", entity.CompanyName);
            parameters.Add("@ContactPerson", entity.ContactPerson);
            parameters.Add("@MobileNo", entity.MobileNo);
            parameters.Add("@EmailId", entity.EmailId);
            parameters.Add("@Address", entity.Address);
            parameters.Add("@StyleNo", entity.StyleNo);
            parameters.Add("@DesignName", entity.DesignName);
            parameters.Add("@ProductType", entity.ProductType);
            parameters.Add("@RatePerPiece", entity.RatePerPiece);
            parameters.Add("@RatePerMeter", entity.RatePerMeter);
            parameters.Add("@NoOfStitches", entity.NoOfStitches);
            parameters.Add("@ChenilleColors", entity.ChenilleColors);
            parameters.Add("@NormalEmbColors", entity.NormalEmbColors);
            parameters.Add("@Quantity", entity.Quantity);
            parameters.Add("@TotalAmount", entity.TotalAmount);
            parameters.Add("@Remarks", entity.Remarks);
            parameters.Add("@Status", entity.Status);
            parameters.Add("@ModifiedBy", entity.ModifiedBy);

            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@StatusMessage", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("USP_RateQuotation_Update", parameters, commandType: CommandType.StoredProcedure);

            var statusCode = parameters.Get<int>("@StatusCode");
            var statusMessage = parameters.Get<string>("@StatusMessage");

            return (statusCode, statusMessage);
        }

        public async Task<(int StatusCode, string StatusMessage)> DeleteAsync(long id, long modifiedBy)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@ModifiedBy", modifiedBy);
            
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@StatusMessage", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("USP_RateQuotation_Delete", parameters, commandType: CommandType.StoredProcedure);

            var statusCode = parameters.Get<int>("@StatusCode");
            var statusMessage = parameters.Get<string>("@StatusMessage");

            return (statusCode, statusMessage);
        }

        public async Task<(RateQuotationEntity? Entity, int StatusCode, string StatusMessage)> GetByIdAsync(long id)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@StatusMessage", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

            var entity = await connection.QueryFirstOrDefaultAsync<RateQuotationEntity>(
                "USP_RateQuotation_GetById", parameters, commandType: CommandType.StoredProcedure);

            var statusCode = parameters.Get<int>("@StatusCode");
            var statusMessage = parameters.Get<string>("@StatusMessage");

            return (entity, statusCode, statusMessage);
        }

        public async Task<(IEnumerable<RateQuotationEntity> Entities, int StatusCode, string StatusMessage)> GetAllAsync()
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@StatusMessage", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

            var entities = await connection.QueryAsync<RateQuotationEntity>(
                "USP_RateQuotation_GetAll", parameters, commandType: CommandType.StoredProcedure);

            var statusCode = parameters.Get<int>("@StatusCode");
            var statusMessage = parameters.Get<string>("@StatusMessage");

            return (entities, statusCode, statusMessage);
        }

        public async Task<(IEnumerable<RateQuotationEntity> Entities, int StatusCode, string StatusMessage)> SearchAsync(Models.DTOs.RateQuotationSearchDto searchDto)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@QuotationNo", searchDto.QuotationNo);
            parameters.Add("@CompanyName", searchDto.CompanyName);
            parameters.Add("@StyleNo", searchDto.StyleNo);
            parameters.Add("@DesignName", searchDto.DesignName);
            parameters.Add("@FromDate", searchDto.FromDate);
            parameters.Add("@ToDate", searchDto.ToDate);
            parameters.Add("@Status", searchDto.Status);

            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@StatusMessage", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

            var entities = await connection.QueryAsync<RateQuotationEntity>(
                "USP_RateQuotation_Search", parameters, commandType: CommandType.StoredProcedure);

            var statusCode = parameters.Get<int>("@StatusCode");
            var statusMessage = parameters.Get<string>("@StatusMessage");

            return (entities, statusCode, statusMessage);
        }

        public async Task<(IEnumerable<RateQuotationEntity> Entities, int TotalRecords, int StatusCode, string StatusMessage)> GetPagedAsync(Models.DTOs.RateQuotationSearchDto searchDto)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", searchDto.PageNumber);
            parameters.Add("@PageSize", searchDto.PageSize);
            parameters.Add("@QuotationNo", searchDto.QuotationNo);
            parameters.Add("@CompanyName", searchDto.CompanyName);
            parameters.Add("@StyleNo", searchDto.StyleNo);
            parameters.Add("@DesignName", searchDto.DesignName);
            parameters.Add("@FromDate", searchDto.FromDate);
            parameters.Add("@ToDate", searchDto.ToDate);
            parameters.Add("@Status", searchDto.Status);

            parameters.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@StatusMessage", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);

            var entities = await connection.QueryAsync<RateQuotationEntity>(
                "USP_RateQuotation_Pagination", parameters, commandType: CommandType.StoredProcedure);

            var totalRecords = parameters.Get<int>("@TotalRecords");
            var statusCode = parameters.Get<int>("@StatusCode");
            var statusMessage = parameters.Get<string>("@StatusMessage");

            return (entities, totalRecords, statusCode, statusMessage);
        }
    }
}
