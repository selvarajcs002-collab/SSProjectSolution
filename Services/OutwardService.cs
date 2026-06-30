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
                string sizeDataJson;
                if (request.ColourBreakdowns != null && request.ColourBreakdowns.Any())
                {
                    sizeDataJson = JsonConvert.SerializeObject(new
                    {
                        colourBreakdowns = request.ColourBreakdowns.Select(c => new
                        {
                            colour = !string.IsNullOrEmpty(c.ColourName) ? c.ColourName : c.Colour,
                            sizes = (c.SizeBreakdowns != null && c.SizeBreakdowns.Any()) 
                                ? c.SizeBreakdowns.Select(s => new
                                {
                                    size = s.SizeName,
                                    count = s.Quantity
                                }).ToList()
                                : c.Sizes?.Select(s => new
                                {
                                    size = s.Size,
                                    count = s.Count
                                }).ToList()
                        }).ToList()
                    });
                }
                else
                {
                    // Fallback to legacy single-colour
                    sizeDataJson = JsonConvert.SerializeObject(new
                    {
                        sizes = request.Sizes?.Select(s => new
                        {
                            size = s.Size,
                            count = s.Count
                        })
                    });
                }

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

                parameters.Add("@DeliveryTo", string.IsNullOrWhiteSpace(request.Outward.DeliveryTo) ? null : request.Outward.DeliveryTo);
                parameters.Add("@PoNo", string.IsNullOrWhiteSpace(request.Outward.PoNo) ? null : request.Outward.PoNo);
                parameters.Add("@Weight", string.IsNullOrWhiteSpace(request.Outward.Weight) ? null : request.Outward.Weight);
                parameters.Add("@NoOfBundles", string.IsNullOrWhiteSpace(request.Outward.NoOfBundles) ? null : request.Outward.NoOfBundles);
                parameters.Add("@SelectedDcNos", request.Outward.SelectedDcNos != null && request.Outward.SelectedDcNos.Any() ? string.Join(",", request.Outward.SelectedDcNos) : null);

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
                bool isInward = mode.ToUpper() == "INWARD";

                // Fetch EntryType and MeterDetails
                using var connection = _dbConnection.CreateConnection();
                var entryType = isInward ? 
                    await connection.QueryFirstOrDefaultAsync<string>("SELECT InwardEntryType FROM Inward WHERE InwardId = @id", new { id }) :
                    await connection.QueryFirstOrDefaultAsync<string>("SELECT OutwardEntryType FROM Outward WHERE OutwardId = @id", new { id });

                var meterDetails = new List<MeterDetailDto>();
                if (entryType == "M") 
                {
                    if (isInward) {
                        meterDetails = (await connection.QueryAsync<MeterDetailDto>(
                            "SELECT IMD_METER_VALUE AS MeterValue, IMD_BITS_COUNT AS BitsCount, IMD_TOTAL_METER AS TotalMeter FROM INWARD_METER_DETAIL WHERE IMD_INWARD_ID = @id", new { id })).ToList();
                    } else {
                        meterDetails = (await connection.QueryAsync<MeterDetailDto>(
                            "SELECT OMD_METER_VALUE AS MeterValue, OMD_BITS_COUNT AS BitsCount, OMD_TOTAL_METER AS TotalMeter FROM OUTWARD_METER_DETAIL WHERE OMD_OUTWARD_ID = @id", new { id })).ToList();
                    }
                }

                var rawData = await _outwardRepository.GetOutwardDetailsRawAsync(id, mode);
                
                if (rawData == null || !rawData.Any()) return null;

                // Mapping and grouping logic using mode-specific ID as the key
                return rawData
                    .Where(x => (isInward ? x.InwardId : x.OutwardId) != null)
                    .GroupBy(x => (int)(isInward ? x.InwardId : x.OutwardId))
                    .Select(g => 
                    {
                        var allSizes = g.Where(s => s.SizeCountId != null).ToList();
                        return new OutwardByDcResponseDto
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
                            EntryType = entryType ?? "S",
                            DeliveryTo = (string?)g.First().DeliveryTo,
                            PoNo = (string?)g.First().PoNo,
                            Weight = (string?)g.First().Weight,
                            NoOfBundles = (string?)g.First().NoOfBundles,
                            SelectedDcNos = !string.IsNullOrEmpty((string?)g.First().SelectedDcNos) 
                                ? ((string)g.First().SelectedDcNos).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() 
                                : new List<string>(),
                            MeterDetails = meterDetails,
                            SizeCounts = allSizes.Select(s => new SizeCountDetailsDto
                            {
                                SizeCountId = s.SizeCountId,
                                Size = s.Size,
                                Count = s.Count
                            }).ToList(),
                            ColourBreakdowns = allSizes
                                .GroupBy(s => (string)s.SizeColour ?? (string)g.First().Colour)
                                .Select(cg => new ColourBreakdownResponseDto
                                {
                                    Colour = cg.Key,
                                    Sizes = cg.Select(s => new SizeCountDetailsDto
                                    {
                                        SizeCountId = s.SizeCountId,
                                        Size = s.Size,
                                        Count = s.Count
                                    }).ToList()
                                }).ToList()
                        };
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
                // Use new request model fields directly since we are moving towards multi-colour
                string sizeDataJson;
                if (request.ColourBreakdowns != null && request.ColourBreakdowns.Any())
                {
                    sizeDataJson = JsonConvert.SerializeObject(new
                    {
                        colourBreakdowns = request.ColourBreakdowns.Select(c => new
                        {
                            colour = !string.IsNullOrEmpty(c.ColourName) ? c.ColourName : c.Colour,
                            sizes = (c.SizeBreakdowns != null && c.SizeBreakdowns.Any()) 
                                ? c.SizeBreakdowns.Select(s => new
                                {
                                    size = s.SizeName,
                                    count = s.Quantity
                                }).ToList()
                                : c.Sizes?.Select(s => new
                                {
                                    size = s.Size,
                                    count = s.Count
                                }).ToList()
                        }).ToList()
                    });
                }
                else
                {
                    sizeDataJson = JsonConvert.SerializeObject(new
                    {
                        sizes = request.SizeCounts?.Select(s => new
                        {
                            size = s.Size,
                            count = s.Count
                        })
                    });
                }

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
                parameters.Add("@DeliveryTo", string.IsNullOrWhiteSpace(request.DeliveryTo) ? null : request.DeliveryTo);
                parameters.Add("@PoNo", string.IsNullOrWhiteSpace(request.PoNo) ? null : request.PoNo);
                parameters.Add("@Weight", string.IsNullOrWhiteSpace(request.Weight) ? null : request.Weight);
                parameters.Add("@NoOfBundles", string.IsNullOrWhiteSpace(request.NoOfBundles) ? null : request.NoOfBundles);
                parameters.Add("@SelectedDcNos", request.SelectedDcNos != null && request.SelectedDcNos.Any() ? string.Join(",", request.SelectedDcNos) : null);
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

        public async Task<System.Collections.Generic.IEnumerable<dynamic>> GetAvailableSizesAsync(int companyId, string styleNo, string designName, string colour)
        {
            try
            {
                return await _outwardRepository.GetAvailableSizesAsync(companyId, styleNo, designName, colour);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetAvailableSizesAsync: " + ex.Message);
            }
        }

        public async Task<System.Collections.Generic.IEnumerable<dynamic>> GetColoursByDcsAsync(int companyId, string styleNo, string designName, System.Collections.Generic.List<string> dcNos)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@CompanyId", companyId);
                parameters.Add("@StyleNo", styleNo);
                parameters.Add("@DesignName", designName);
                parameters.Add("@DcNos", string.Join(",", dcNos));

                string sql = @"
                    SELECT DISTINCT
                        CASE WHEN i.Colour = 'MULTI' THEN isc.Colour ELSE i.Colour END AS colour
                    FROM Inward i
                    LEFT JOIN InwardSizeCount isc ON i.InwardId = isc.InwardId
                    WHERE i.CompanyId = @CompanyId 
                      AND i.StyleNo = @StyleNo 
                      AND i.DesignName = @DesignName
                      AND i.InwardDcNo IN (SELECT value FROM STRING_SPLIT(@DcNos, ','))
                      AND (i.Colour != 'MULTI' OR isc.Colour IS NOT NULL)";

                var result = await connection.QueryAsync<dynamic>(sql, parameters);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetColoursByDcsAsync: " + ex.Message);
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
                parameters.Add("@DeliveryTo", string.IsNullOrWhiteSpace(request.DeliveryTo) ? null : request.DeliveryTo);
                parameters.Add("@PoNo", string.IsNullOrWhiteSpace(request.PoNo) ? null : request.PoNo);
                parameters.Add("@Weight", string.IsNullOrWhiteSpace(request.Weight) ? null : request.Weight);
                parameters.Add("@NoOfBundles", string.IsNullOrWhiteSpace(request.NoOfBundles) ? null : request.NoOfBundles);
                parameters.Add("@UploadURL", request.UploadURL == "null" || string.IsNullOrWhiteSpace(request.UploadURL) ? null : request.UploadURL);
                parameters.Add("@Status", string.IsNullOrWhiteSpace(request.Status) ? null : request.Status);
                parameters.Add("@Remarks", string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks);
                
                DateTime? parsedOutwardDate = null;
                if (!string.IsNullOrWhiteSpace(request.OutwardDate) && DateTime.TryParse(request.OutwardDate, out var date))
                    parsedOutwardDate = date;
                    
                parameters.Add("@OutwardDate", parsedOutwardDate);
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

        // ── Additional Details ─────────────────────────────────────────────────

        public async Task<dynamic> GetAdditionalDetailsOptionsAsync(int companyId)
        {
            try
            {
                return await _outwardRepository.GetAdditionalDetailsOptionsAsync(companyId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetAdditionalDetailsOptionsAsync: " + ex.Message);
            }
        }
    }
}
