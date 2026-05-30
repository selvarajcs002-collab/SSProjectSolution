using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using SSProjectSolution.Data;
using SSProjectSolution.Repositories;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public class OutwardService : IOutwardService
    {
        private readonly DapperDBConnection _dbConnection;
        private readonly IOutwardRepository _outwardRepository;

        public OutwardService(DapperDBConnection dbConnection, IOutwardRepository outwardRepository)
        {
            _dbConnection = dbConnection;
            _outwardRepository = outwardRepository;
        }

        // ── Size-Based (existing — untouched) ──────────────────────────────────

        public async Task<OutwardResponse> SaveOutwardAsync(OutwardRequest request)
        {
            try
            {
                // ? MATCH SP EXPECTATION (IMPORTANT)
                var sizeDataJson = JsonConvert.SerializeObject(new
                {
                    sizes = request.Sizes.Select(s => new
                    {
                        size = s.Size,
                        count = s.Count
                    })
                });

                var parameters = new DynamicParameters();

                parameters.Add("@Mode", request.Outward.Mode);

                parameters.Add("@OutwardId",
                    request.Outward.Mode == "INSERT" ? null : request.Outward.OutwardId,
                    dbType: DbType.Int32,
                    direction: ParameterDirection.InputOutput);

                parameters.Add("@CompanyId", request.Outward.CompanyId);
                parameters.Add("@Colour", request.Outward.Colour);
                parameters.Add("@DesignName", request.Outward.DesignName);
                parameters.Add("@StyleNo", request.Outward.StyleNo);
                parameters.Add("@UploadURL",
                    request.Outward.UploadURL == "null" ? null : request.Outward.UploadURL);
                parameters.Add("@CreatedBy", request.Outward.CreatedBy);
                parameters.Add("@Status", request.Outward.Status);

                // ?? CRITICAL FIX
                parameters.Add("@SizeData", sizeDataJson, DbType.String);

                parameters.Add("@OutwardDcNo",
                    dbType: DbType.String,
                    direction: ParameterDirection.Output,
                    size: 50);

                // ? USE Repository method
                var response = await _outwardRepository.SaveOutwardAsync(parameters);

                if (response != null)
                {
                    if (response.OutwardId == 0)
                    {
                        var outId = parameters.Get<int?>("@OutwardId");
                        if (outId.HasValue) response.OutwardId = outId.Value;
                    }
                    if (string.IsNullOrEmpty(response.OutwardDcNo))
                    {
                        var outDc = parameters.Get<string>("@OutwardDcNo");
                        if (!string.IsNullOrEmpty(outDc)) response.OutwardDcNo = outDc;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return new OutwardResponse
                {
                    Success = false,
                    Message = "Error in SaveOutwardAsync: " + ex.Message
                };
            }
        }

        public async Task<OutwardByDcResponseDto?> GetOutwardByDcNoAsync(int id, string mode)
        {
            try
            {
                var rawData = await _outwardRepository.GetOutwardDetailsRawAsync(id, mode);
                
                if (rawData == null || !rawData.Any()) return null;

                // Mapping and grouping logic using mode-specific ID as the key
                bool isInward = mode.ToUpper() == "INWARD";
                
                return rawData
                    .Where(x => (isInward ? x.InwardId : x.OutwardId) != null)
                    .GroupBy(x => (int)(isInward ? x.InwardId : x.OutwardId))
                    .Select(g => new OutwardByDcResponseDto
                    {
                        Id = g.Key,
                        CompanyName = g.First().CompanyName,
                        CompanyId = g.First().CompanyId,
                        Colour = g.First().Colour,
                        DesignName = g.First().DesignName,
                        StyleNo = g.First().StyleNo,
                        UploadURL = g.First().UploadURL,
                        CreatedBy = g.First().CreatedBy?.ToString(),
                        CreatedDate = g.First().CreatedDate,
                        UpdatedDate = g.First().UpdatedDate,
                        DcNo = isInward ? g.First().InwardDcNo : g.First().OutwardDcNo,
                        Status = g.First().Status,
                        SizeCounts = g.Where(s => s.SizeCountId != null).Select(s => new SizeCountDetailsDto
                        {
                            SizeCountId = s.SizeCountId,
                            Size = s.Size,
                            Count = s.Count
                        }).ToList()
                    }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetOutwardByDcNoAsync: " + ex.Message);
            }
        }

        public async Task<OutwardResponse> UpdateOutwardAsync(OutwardUpdateRequest request)
        {
            try
            {
                var sizeDataJson = JsonConvert.SerializeObject(new
                {
                    sizes = request.SizeCounts.Select(s => new
                    {
                        size = s.Size,
                        count = s.Count
                    })
                });

                var parameters = new DynamicParameters();
                parameters.Add("@Mode", "UPDATE");
                parameters.Add("@OutwardId", request.OutwardId, dbType: DbType.Int32, direction: ParameterDirection.InputOutput);
                parameters.Add("@CompanyId", request.CompanyId);
                parameters.Add("@Colour", request.Colour);
                parameters.Add("@DesignName", request.DesignName);
                parameters.Add("@StyleNo", request.StyleNo);
                parameters.Add("@UploadURL", request.UploadURL == "null" ? null : request.UploadURL);
                parameters.Add("@CreatedBy", request.CreatedBy);
                parameters.Add("@Status", request.Status);
                parameters.Add("@SizeData", sizeDataJson, DbType.String);

                parameters.Add("@OutwardDcNo", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                var response = await _outwardRepository.SaveOutwardAsync(parameters);
                
                if (response != null)
                {
                    if (response.OutwardId == 0)
                    {
                        var outId = parameters.Get<int?>("@OutwardId");
                        if (outId.HasValue) response.OutwardId = outId.Value;
                    }
                    if (string.IsNullOrEmpty(response.OutwardDcNo))
                    {
                        var outDc = parameters.Get<string>("@OutwardDcNo");
                        if (!string.IsNullOrEmpty(outDc)) response.OutwardDcNo = outDc;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return new OutwardResponse
                {
                    Success = false,
                    Message = "Error in UpdateOutwardAsync: " + ex.Message
                };
            }
        }

        // ── Meter-Based (new — isolated) ───────────────────────────────────────

        public async Task<OutwardMeterResponse> SaveMeterOutwardAsync(OutwardMeterSaveRequest request)
        {
            try
            {
                // Validate mode
                if (string.IsNullOrWhiteSpace(request.Mode) ||
                    (request.Mode != "INSERT" && request.Mode != "UPDATE"))
                {
                    return new OutwardMeterResponse
                    {
                        Success = false,
                        Message = "Mode must be INSERT or UPDATE"
                    };
                }

                // Build UDTT DataTable — backend recalculates TotalMeter inside the SP
                var dt = new DataTable();
                dt.Columns.Add("MeterValue", typeof(decimal));
                dt.Columns.Add("BitsCount", typeof(decimal));

                foreach (var detail in request.MeterDetails)
                {
                    if (detail.MeterValue <= 0 || detail.BitsCount <= 0)
                        continue; // SP also validates, but filter obvious bad rows
                    dt.Rows.Add(detail.MeterValue, detail.BitsCount);
                }

                if (dt.Rows.Count == 0)
                {
                    return new OutwardMeterResponse
                    {
                        Success = false,
                        Message = "No valid meter details provided. MeterValue and BitsCount must be greater than 0."
                    };
                }

                var parameters = new DynamicParameters();
                parameters.Add("@OutwardId",
                    request.Mode == "INSERT" ? 0 : request.OutwardId,
                    dbType: DbType.Int32);
                parameters.Add("@CompanyId", request.CompanyId);
                parameters.Add("@StyleId", request.StyleId);
                parameters.Add("@DesignId", request.DesignId);
                parameters.Add("@Colour", request.Colour.Trim());
                parameters.Add("@DesignName", request.DesignName.Trim());
                parameters.Add("@StyleNo", request.StyleNo.Trim());
                parameters.Add("@EntryType", request.EntryType);
                parameters.Add("@Mode", request.Mode);
                parameters.Add("@CreatedBy", request.CreatedBy);
                parameters.Add("@MeterDetails", dt.AsTableValuedParameter("OutwardMeterDetailType"));

                var response = await _outwardRepository.SaveMeterOutwardAsync(parameters);
                return response;
            }
            catch (Exception ex)
            {
                return new OutwardMeterResponse
                {
                    Success = false,
                    Message = "Error in SaveMeterOutwardAsync: " + ex.Message
                };
            }
        }
    }
}
