using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Request;
using SSProjectSolution.Repositories;
using SSProjectSolution.Models.DTOs;
using System.Data;

namespace SSProjectSolution.Services
{
    public class InwardService : IInwardService
    {
        private readonly DapperDBConnection _dbConnection;
        private readonly IInwardRepository _inwardRepository;

        public InwardService(DapperDBConnection dbConnection, IInwardRepository inwardRepository)
        {
            _dbConnection = dbConnection;
            _inwardRepository = inwardRepository;
        }

        public async Task<int> SaveInwardAsync(InwardCreateDto request)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@Colour", request.Colour);
            parameters.Add("@DesignName", request.DesignName);
            parameters.Add("@StyleNo", request.StyleNo);
            parameters.Add("@InwardDcNo", request.InwardDcNo);
            parameters.Add("@PoNo", request.PoNo);
            parameters.Add("@UploadURL", null);
            parameters.Add("@CreatedBy", request.CreatedBy);
            parameters.Add("@InwardDate", request.InwardDate);

            return await connection.QueryFirstOrDefaultAsync<int>(
                SPConstants.InsertInward,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task SaveInwardSizeCountsAsync(int inwardId, string styleNo, string designName, string colour, List<SizeDto> sizes)
        {
            using var connection = _dbConnection.CreateConnection();
            
            // Create DataTable for UDTT
            var dt = new DataTable();
            dt.Columns.Add("Size", typeof(string));
            dt.Columns.Add("Count", typeof(int));

            foreach (var size in sizes)
            {
                dt.Rows.Add(size.Size, size.Count);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@InwardId", inwardId);
            parameters.Add("@StyleNo", styleNo);
            parameters.Add("@DesignName", designName);
            parameters.Add("@Colour", colour);
            parameters.Add("@SizeCounts", dt.AsTableValuedParameter("SizeCountType"));

            await connection.ExecuteAsync(
                SPConstants.InsertInwardSizeCounts,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<(int InwardId, string Message)> SaveMeterInwardAsync(InwardMeterSaveRequest request)
        {
            using var connection = _dbConnection.CreateConnection();
            
            // Create DataTable for UDTT
            var dt = new DataTable();
            dt.Columns.Add("MeterValue", typeof(decimal));
            dt.Columns.Add("BitsCount", typeof(decimal));

            foreach (var detail in request.MeterDetails)
            {
                dt.Rows.Add(detail.MeterValue, detail.BitsCount);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@InwardId", request.InwardId);
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@Colour", request.Colour);
            parameters.Add("@DesignName", request.DesignName);
            parameters.Add("@StyleNo", request.StyleNo);
            parameters.Add("@InwardDcNo", request.InwardDcNo);
            parameters.Add("@PoNo", request.PoNo);
            parameters.Add("@EntryType", request.EntryType);
            parameters.Add("@CreatedBy", request.CreatedBy);
            parameters.Add("@MeterDetails", dt.AsTableValuedParameter("MeterDetailType"));

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                SPConstants.SaveInwardMeter,
                parameters,
                commandType: CommandType.StoredProcedure);

            return (result?.InwardId ?? 0, result?.Message ?? "Failed to save Meter Inward");
        }

        public async Task<IEnumerable<SizeResponseDto>> GetSizesByColourStyleAsync(int companyId, string colour, string styleNo)
        {
            return await _inwardRepository.GetSizesByColourStyleAsync(companyId, colour, styleNo);
        }

        public async Task<IEnumerable<MeterResponseDto>> GetMetersByColourStyleAsync(int companyId, string colour, string styleNo)
        {
            return await _inwardRepository.GetMetersByColourStyleAsync(companyId, colour, styleNo);
        }

        public async Task<InwardByDcResponseDto> GetInwardByCompanyAndDcAsync(int companyId, string inwardDcNo)
        {
            return await _inwardRepository.GetInwardByCompanyAndDcAsync(companyId, inwardDcNo);
        }

        public async Task<string> UpdateInwardAsync(InwardUpdateDto inwardUpdate)
        {
            return await _inwardRepository.UpdateInwardAsync(inwardUpdate);
        }

        public async Task<IEnumerable<DesignStyleColourDto>> GetDesignStyleColourByCompanyAsync(int companyId)
        {
            return await _inwardRepository.GetDesignStyleColourByCompanyAsync(companyId);
        }
    }
}
