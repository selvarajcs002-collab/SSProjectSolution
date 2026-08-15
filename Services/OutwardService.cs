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
                parameters.Add("@Remarks", string.IsNullOrWhiteSpace(request.Outward.Remarks) ? null : request.Outward.Remarks);
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
                            "SELECT OMD_METER_VALUE AS MeterValue, OMD_BITS_COUNT AS BitsCount, OMD_PIECES_COUNT AS PiecesCount, OMD_TOTAL_METER AS TotalMeter FROM OUTWARD_METER_DETAIL WHERE OMD_OUTWARD_ID = @id", new { id })).ToList();
                    }
                }

                var rawData = await _outwardRepository.GetOutwardDetailsRawAsync(id, mode);
                
                if (rawData == null || !rawData.Any()) return null;

                var idProp = isInward ? "InwardId" : "OutwardId";
                var dcNoProp = isInward ? "InwardDcNo" : "OutwardDcNo";

                var validRows = rawData
                    .Select(x => x as IDictionary<string, object>)
                    .Where(dict => dict != null && dict.ContainsKey(idProp) && dict[idProp] != null)
                    .ToList();

                if (!validRows.Any()) return null;

                return validRows
                    .GroupBy(dict => Convert.ToInt32(dict![idProp]))
                    .Select(g => 
                    {
                        var firstRowDict = g.First();
                        DateTime? inwardDateVal = null;
                        if (firstRowDict != null)
                        {
                            foreach (var kvp in firstRowDict)
                            {
                                if (string.Equals(kvp.Key, "InwardDate", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (kvp.Value != null && DateTime.TryParse(kvp.Value.ToString(), out var parsedDate))
                                    {
                                        inwardDateVal = parsedDate;
                                    }
                                    break;
                                }
                            }
                        }

                        var allSizes = g.Where(dict => 
                            dict != null && 
                            dict.ContainsKey("SizeCountId") && dict["SizeCountId"] != null && 
                            dict.ContainsKey("Size") && dict["Size"] != null
                        ).ToList();

                        string mainColour = firstRowDict != null && firstRowDict.ContainsKey("Colour") && firstRowDict["Colour"] != null 
                            ? firstRowDict["Colour"]?.ToString() ?? "" 
                            : "";

                        return new OutwardByDcResponseDto
                        {
                            Id = g.Key,
                            CompanyName = firstRowDict != null && firstRowDict.ContainsKey("CompanyName") && firstRowDict["CompanyName"] != null ? firstRowDict["CompanyName"]?.ToString() : null,
                            CompanyId = firstRowDict != null && firstRowDict.ContainsKey("CompanyId") && firstRowDict["CompanyId"] != null ? Convert.ToInt32(firstRowDict["CompanyId"]) : 0,
                            Colour = mainColour,
                            DesignName = firstRowDict != null && firstRowDict.ContainsKey("DesignName") && firstRowDict["DesignName"] != null ? firstRowDict["DesignName"]?.ToString() : null,
                            StyleNo = firstRowDict != null && firstRowDict.ContainsKey("StyleNo") && firstRowDict["StyleNo"] != null ? firstRowDict["StyleNo"]?.ToString() : null,
                            UploadURL = firstRowDict != null && firstRowDict.ContainsKey("UploadURL") && firstRowDict["UploadURL"] != null ? firstRowDict["UploadURL"]?.ToString() : null,
                            CreatedBy = firstRowDict != null && firstRowDict.ContainsKey("CreatedBy") && firstRowDict["CreatedBy"] != null ? firstRowDict["CreatedBy"]?.ToString() : null,
                            CreatedDate = firstRowDict != null && firstRowDict.ContainsKey("CreatedDate") && firstRowDict["CreatedDate"] != null && DateTime.TryParse(firstRowDict["CreatedDate"]?.ToString(), out var cd) ? cd : DateTime.MinValue,
                            InwardDate = inwardDateVal ?? (firstRowDict != null && firstRowDict.ContainsKey("CreatedDate") && firstRowDict["CreatedDate"] != null && DateTime.TryParse(firstRowDict["CreatedDate"]?.ToString(), out var cd2) ? cd2 : DateTime.MinValue),
                            UpdatedDate = firstRowDict != null && firstRowDict.ContainsKey("UpdatedDate") && firstRowDict["UpdatedDate"] != null && DateTime.TryParse(firstRowDict["UpdatedDate"]?.ToString(), out var ud) ? ud : null,
                            DcNo = firstRowDict != null && firstRowDict.ContainsKey(dcNoProp) && firstRowDict[dcNoProp] != null ? firstRowDict[dcNoProp]?.ToString() : null,
                            Status = firstRowDict != null && firstRowDict.ContainsKey("Status") && firstRowDict["Status"] != null ? firstRowDict["Status"]?.ToString() : null,
                            EntryType = entryType ?? "S",
                            DeliveryTo = firstRowDict?.ContainsKey("DeliveryTo") == true ? firstRowDict["DeliveryTo"]?.ToString() : null,
                            PoNo = firstRowDict?.ContainsKey("PoNo") == true ? firstRowDict["PoNo"]?.ToString() : null,
                            Weight = firstRowDict?.ContainsKey("Weight") == true ? firstRowDict["Weight"]?.ToString() : null,
                            NoOfBundles = firstRowDict?.ContainsKey("NoOfBundles") == true ? firstRowDict["NoOfBundles"]?.ToString() : null,
                            Remarks = firstRowDict?.ContainsKey("Remarks") == true ? firstRowDict["Remarks"]?.ToString() : null,
                            SelectedDcNos = (firstRowDict?.ContainsKey("SelectedDcNos") == true && !string.IsNullOrEmpty(firstRowDict["SelectedDcNos"]?.ToString()))
                                ? firstRowDict["SelectedDcNos"]!.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() 
                                : new List<string>(),
                            MeterDetails = meterDetails,
                            SizeCounts = allSizes.Select(dict => new SizeCountDetailsDto
                            {
                                SizeCountId = Convert.ToInt32(dict!["SizeCountId"]),
                                Size = dict["Size"]?.ToString() ?? "",
                                Count = dict.ContainsKey("Count") && dict["Count"] != null ? Convert.ToInt32(dict["Count"]) : 0
                            }).ToList(),
                            ColourBreakdowns = allSizes
                                .GroupBy(dict => 
                                {
                                    if (dict != null && dict.ContainsKey("SizeColour") && dict["SizeColour"] != null && !string.IsNullOrWhiteSpace(dict["SizeColour"]?.ToString()))
                                    {
                                        return dict["SizeColour"]!.ToString()!;
                                    }
                                    return mainColour;
                                })
                                .Select(cg => new ColourBreakdownResponseDto
                                {
                                    Colour = cg.Key,
                                    Sizes = cg.Select(dict => new SizeCountDetailsDto
                                    {
                                        SizeCountId = Convert.ToInt32(dict!["SizeCountId"]),
                                        Size = dict["Size"]?.ToString() ?? "",
                                        Count = dict.ContainsKey("Count") && dict["Count"] != null ? Convert.ToInt32(dict["Count"]) : 0
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
                parameters.Add("@Remarks", string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks);
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
                dt.Columns.Add("PiecesCount", typeof(decimal));

                foreach (var detail in request.MeterDetails)
                {
                    if (detail.MeterValue <= 0 || detail.BitsCount <= 0)
                        continue; // SP also validates, but filter obvious bad rows
                    dt.Rows.Add(detail.MeterValue, detail.BitsCount, (object)detail.PiecesCount ?? DBNull.Value);
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
                parameters.Add("@Remarks", string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks);
                parameters.Add("@SelectedDcNos", request.SelectedDcNos != null && request.SelectedDcNos.Any() ? string.Join(",", request.SelectedDcNos) : null);
                parameters.Add("@UploadURL", request.UploadURL == "null" || string.IsNullOrWhiteSpace(request.UploadURL) ? null : request.UploadURL);
                parameters.Add("@Status", string.IsNullOrWhiteSpace(request.Status) ? null : request.Status);
                
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

        // ── Lot Completion ─────────────────────────────────────────────────────

        public async Task<dynamic> MarkLotCompletedAsync(LotCompletedDto payload)
        {
            try
            {
                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@CompanyId", payload.CompanyId);
                parameters.Add("@StyleNo", payload.StyleNo);
                parameters.Add("@DesignName", payload.DesignName);
                parameters.Add("@Colour", payload.Colour);
                parameters.Add("@PoNo", payload.PoNo);
                parameters.Add("@IsDeliveryChallan", payload.IsDeliveryChallan ? 1 : 0);
                parameters.Add("@SelectedDcNos", payload.SelectedDcNos != null && payload.SelectedDcNos.Any() ? string.Join(",", payload.SelectedDcNos) : null);
                parameters.Add("@EntryType", payload.EntryType);
                
                string consumedSizesJson = payload.ConsumedSizes != null && payload.ConsumedSizes.Any()
                    ? System.Text.Json.JsonSerializer.Serialize(payload.ConsumedSizes)
                    : null;
                parameters.Add("@ConsumedSizesJson", consumedSizesJson);

                string consumedMetersJson = payload.ConsumedMeters != null && payload.ConsumedMeters.Any()
                    ? System.Text.Json.JsonSerializer.Serialize(payload.ConsumedMeters)
                    : null;
                parameters.Add("@ConsumedMetersJson", consumedMetersJson);

                return await _outwardRepository.MarkLotCompletedAsync(parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in MarkLotCompletedAsync: " + ex.Message);
            }
        }

        public async Task<dynamic> MarkInwardInactiveAsync(InwardStatusUpdateDto payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CompanyId", payload.CompanyId);
                parameters.Add("@StyleNo", payload.StyleNo);
                parameters.Add("@DesignName", payload.DesignName);
                parameters.Add("@Colour", payload.Colour);

                return await _outwardRepository.MarkInwardInactiveAsync(parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in MarkInwardInactiveAsync: " + ex.Message);
            }
        }

        public async Task<dynamic> MarkInwardInactiveByDcNoAsync(InwardStatusUpdateByDcNoDto payload)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CompanyId", payload.CompanyId);
                parameters.Add("@StyleNo", payload.StyleNo);
                parameters.Add("@DesignName", payload.DesignName);
                parameters.Add("@Colour", payload.Colour);
                parameters.Add("@InwardDcNo", payload.InwardDcNo);

                return await _outwardRepository.MarkInwardInactiveByDcNoAsync(parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in MarkInwardInactiveByDcNoAsync: " + ex.Message);
            }
        }
    }
}
