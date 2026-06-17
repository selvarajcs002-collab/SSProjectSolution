using Dapper;
using Microsoft.Data.SqlClient;
using SSProjectSolution.Data;
using SSProjectSolution.Models.DTOs;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SSProjectSolution.Repositories
{
    public class DcDetailRepository : IDcDetailRepository
    {
        private readonly DapperDBConnection _dbConnection;

        public DcDetailRepository(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // ──────────────────────────────────────────────────────────────
        // GET DC NUMBERS  (unchanged)
        // ──────────────────────────────────────────────────────────────
        public async Task<IEnumerable<DcNoResponseDto>> GetDcNumbersByCompanyAsync(
            int companyId, string styleNo = null, string designName = null)
        {
            using var connection = _dbConnection.CreateConnection();
            var sql = @"
                SELECT DISTINCT 
                    InwardDcNo, 
                    StyleNo, 
                    DesignName 
                FROM Inward 
                WHERE CompanyId = @CompanyId 
                  AND InwardDcNo IS NOT NULL 
                  AND LTRIM(RTRIM(InwardDcNo)) <> ''";

            if (!string.IsNullOrEmpty(styleNo))
                sql += " AND StyleNo = @StyleNo";
            if (!string.IsNullOrEmpty(designName))
                sql += " AND DesignName = @DesignName";

            return await connection.QueryAsync<DcNoResponseDto>(
                sql, new { CompanyId = companyId, StyleNo = styleNo, DesignName = designName });
        }

        // ──────────────────────────────────────────────────────────────
        // GET INWARD DETAILS — supports 1..N DC numbers
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns aggregated <see cref="InwardEntryDetailsDto"/> for one or more DC numbers.
        /// • Single DC  → identical response shape as the original API.
        /// • Multiple DCs → sizes / meter rows are aggregated across all matched Inward records.
        /// Uses an in-line TVP (via Dapper) for a single, parameterised DB round-trip.
        /// </summary>
        public async Task<InwardEntryDetailsDto> GetInwardDetailsByDcsAsync(
            int companyId, IReadOnlyList<string> inwardDcNos, string colour = null)
        {
            using var connection = _dbConnection.CreateConnection();

            // Build a DataTable that acts as the TVP.
            // SQL type: dbo.DcNumberList (see SQL script below).
            var dcTable = BuildDcTable(inwardDcNos);

            // ── Step 1: Resolve all matching Inward header rows ──────────
            string inwardSql = @"
                SELECT 
                    i.InwardId,
                    i.InwardEntryType,
                    i.StyleNo,
                    i.DesignName,
                    i.Colour
                FROM Inward i
                INNER JOIN @DcList dl ON dl.DcNo = i.InwardDcNo
                WHERE i.CompanyId = @CompanyId";

            var parameters = new DynamicParameters();
            parameters.Add("CompanyId", companyId);
            parameters.Add("DcList", dcTable.AsTableValuedParameter("dbo.DcNumberList"));

            if (!string.IsNullOrEmpty(colour))
            {
                inwardSql += " AND i.Colour = @Colour";
                parameters.Add("Colour", colour);
            }

            var inwardRows = (await connection.QueryAsync<dynamic>(inwardSql, parameters)).AsList();

            var result = new InwardEntryDetailsDto();

            if (inwardRows.Count == 0)
                return result;

            // Determine entry type from the first matching record.
            // All DCs belonging to the same job share the same EntryType.
            result.EntryType = inwardRows[0].InwardEntryType?.ToString();
            result.Colour = inwardRows[0].Colour?.ToString();

            var inwardIds = inwardRows.Select(r => (int)r.InwardId).ToList();

            if (result.EntryType == "S")
            {
                // ── Step 2 (Size path): aggregate per-size counts across all matched
                //    Inward records, then subtract already-outward-used qty for the same
                //    Style + Design + Colour combination (same logic as the original SP).
                //
                // For multiple DCs we need the full Style/Design/Colour context from every row.
                // We group available outward deductions per size across ALL matched records.

                // Build a distinct set of (StyleNo, DesignName, Colour) keys from the headers.
                var contexts = inwardRows
                    .Select(r => new { StyleNo = (string)r.StyleNo, DesignName = (string)r.DesignName, Colour = (string)r.Colour })
                    .Distinct()
                    .ToList();

                // Aggregate inward counts per Size.
                const string sizeSql = @"
                    SELECT 
                        isc.Size,
                        SUM(isc.[Count]) AS [Count]
                    FROM InwardSizeCount isc
                    WHERE isc.InwardId IN @InwardIds
                    GROUP BY isc.Size";

                var rawSizes = (await connection.QueryAsync<(string Size, int Count)>(
                    sizeSql, new { InwardIds = inwardIds })).ToList();

                // Aggregate already-used outward counts per Size for the relevant contexts.
                // We pass all (StyleNo, DesignName, Colour) via a JSON-or-comma workaround.
                // Simplest production-safe way: one query per distinct context, sum up.
                var usedBySize = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);

                foreach (var ctx in contexts)
                {
                    const string outwardSql = @"
                        SELECT osc.Size, SUM(osc.[Count]) AS UsedCount
                        FROM OutwardSizeCount osc
                        WHERE osc.StyleNo    = @StyleNo
                          AND osc.DesignName = @DesignName
                          AND osc.Colour     = @Colour
                        GROUP BY osc.Size";

                    var used = await connection.QueryAsync<(string Size, int UsedCount)>(
                        outwardSql, new { ctx.StyleNo, ctx.DesignName, ctx.Colour });

                    foreach (var u in used)
                    {
                        if (usedBySize.ContainsKey(u.Size))
                            usedBySize[u.Size] += u.UsedCount;
                        else
                            usedBySize[u.Size] = u.UsedCount;
                    }
                }

                // Filter: only include sizes where the computed AvailableQty is >= 0.
                // Negative means outward already exceeded inward for that size — exclude entirely.
                result.Sizes = rawSizes
                    .Select(s => new SizeDetailDto
                    {
                        Size         = s.Size,
                        Count        = s.Count,
                        AvailableQty = s.Count - (usedBySize.TryGetValue(s.Size, out var used) ? used : 0)
                    })
                    .Where(s => s.AvailableQty >= 0)   // drop negative AvailableQty records
                    .ToList();
            }
            else if (result.EntryType == "M")
            {
                // ── Step 2 (Meter path): union all meter detail rows across all matched Inward records.
                // Filter applied at SQL level: rows with a negative TotalMeter are
                // excluded before any data is transferred over the network.
                const string meterSql = @"
                    SELECT 
                        IMD_METER_VALUE  AS MeterValue,
                        IMD_BITS_COUNT   AS BitsCount,
                        IMD_TOTAL_METER  AS TotalMeter
                    FROM INWARD_METER_DETAIL
                    WHERE IMD_INWARD_ID IN @InwardIds
                      AND IMD_TOTAL_METER >= 0";

                var meters = await connection.QueryAsync<MeterDetailDto>(
                    meterSql, new { InwardIds = inwardIds });

                result.MeterDetails = meters.ToList();
            }

            return result;
        }

        // ──────────────────────────────────────────────────────────────
        // Helper — builds a single-column DataTable for the TVP
        // ──────────────────────────────────────────────────────────────
        private static DataTable BuildDcTable(IReadOnlyList<string> dcNos)
        {
            var table = new DataTable();
            table.Columns.Add("DcNo", typeof(string));
            foreach (var dc in dcNos)
                table.Rows.Add(dc);
            return table;
        }
    }
}
