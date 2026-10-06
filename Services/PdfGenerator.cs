using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SSProjectSolution.Documents;
using SSProjectSolution.Request;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;

namespace SSProjectSolution.Services
{
    public class PdfGenerator : IPdfGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly ICompanyService _companyService;
        private readonly ILogger<PdfGenerator> _logger;

        public PdfGenerator(IConfiguration configuration, ICompanyService companyService, ILogger<PdfGenerator> logger)
        {
            _configuration = configuration;
            _companyService = companyService;
            _logger = logger;
        }

        public async Task<byte[]> GeneratePdfAsync(JObject payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload), "Invalid request payload");

            await ApplyCompanyGstAsync(payload);

            string entryType = payload.Value<string>("entryType") ?? "S";

            if (entryType == "M")
            {
                return await Task.Run(() => GenerateMeterPdf(payload));
            }
            else
            {
                return await Task.Run(() => GenerateSizePdf(payload));
            }
        }

        private byte[] GenerateMeterPdf(JObject payload)
        {
            var meterRequest = payload.ToObject<GenerateMeterDcRequest>();
            if (meterRequest == null)
                throw new InvalidOperationException("Failed to deserialize meter request");

            if (string.IsNullOrEmpty(meterRequest.CompanyName))
                meterRequest.CompanyName = payload.Value<string>("receiverName") ?? string.Empty;
                
            if (string.IsNullOrEmpty(meterRequest.Address))
                meterRequest.Address = payload.Value<string>("receiverAddress") ?? string.Empty;

            meterRequest.GstNo = ReadPayloadGst(payload);

            if (string.IsNullOrEmpty(meterRequest.Date))
                meterRequest.Date = payload.Value<string>("date") ?? string.Empty;

            var itemsArray = payload.Value<JArray>("items");
            if (itemsArray != null && itemsArray.Count > 0)
            {
                var firstItem = itemsArray[0] as JObject;
                if (firstItem != null)
                {
                    meterRequest.Design = firstItem.Value<string>("designName") ?? string.Empty;
                    meterRequest.Style = firstItem.Value<string>("styleNo") ?? string.Empty;
                    meterRequest.Color = firstItem.Value<string>("colour") ?? string.Empty;
                }
            }

            var meterDetailsArray = payload.Value<JArray>("meterDetails");
            if (meterDetailsArray != null)
            {
                meterRequest.Items = meterDetailsArray.ToObject<List<MeterDcItem>>() ?? new List<MeterDcItem>();
            }

            meterRequest.TotalMeterSum = payload.Value<decimal>("totalMeterSum");

            if (meterRequest.Items == null || !meterRequest.Items.Any())
                throw new InvalidOperationException("Meter details cannot be empty");

            var document = new MeterDeliveryChallanDocument(meterRequest, _configuration);
            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        private byte[] GenerateSizePdf(JObject payload)
        {
            var sizeRequest = payload.ToObject<GenerateDcRequest>();
            if (sizeRequest == null)
                throw new InvalidOperationException("Failed to deserialize size request");

            if (string.IsNullOrEmpty(sizeRequest.CompanyName))
                sizeRequest.CompanyName = payload.Value<string>("receiverName") ?? string.Empty;

            if (string.IsNullOrEmpty(sizeRequest.Address))
                sizeRequest.Address = payload.Value<string>("receiverAddress") ?? string.Empty;

            sizeRequest.GstNo = ReadPayloadGst(payload);

            if (string.IsNullOrEmpty(sizeRequest.Date))
                sizeRequest.Date = payload.Value<string>("date") ?? string.Empty;

            var colourBreakdownsArray = payload.Value<JArray>("colourBreakdowns");
            if (colourBreakdownsArray != null)
            {
                sizeRequest.ColourBreakdowns = new List<DcColourBreakdown>();
                foreach (var colourToken in colourBreakdownsArray)
                {
                    var colourObj = colourToken as JObject;
                    if (colourObj == null) continue;

                    var colourBreakdown = new DcColourBreakdown
                    {
                        ColourName = colourObj.Value<string>("colourName") ?? colourObj.Value<string>("colour") ?? string.Empty,
                        Sizes = new List<DcSizeBreakdown>()
                    };

                    foreach (var property in colourObj.Properties())
                    {
                        var propertyName = property.Name;
                        if (propertyName.Equals("colourName", StringComparison.OrdinalIgnoreCase) ||
                            propertyName.Equals("colour", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        int quantity = 0;
                        if (property.Value.Type == JTokenType.Integer)
                        {
                            quantity = property.Value.Value<int>();
                        }

                        if (quantity > 0)
                        {
                            colourBreakdown.Sizes.Add(new DcSizeBreakdown
                            {
                                SizeName = propertyName,
                                Quantity = quantity
                            });
                        }
                    }
                    sizeRequest.ColourBreakdowns.Add(colourBreakdown);
                }
            }

            var document = new DeliveryChallanDocument(sizeRequest, _configuration);
            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        private async Task ApplyCompanyGstAsync(JObject payload)
        {
            var companyId = payload.Value<int?>("companyId") ?? payload.Value<int?>("CompanyId") ?? 0;
            var challanNo = payload.Value<string>("dcNo") ?? payload.Value<string>("DcNo") ?? string.Empty;

            if (companyId <= 0)
            {
                payload["gstNo"] = string.Empty;
                _logger.LogWarning(
                    "Delivery challan PDF has no CompanyId. ChallanNo={ChallanNo}. GST was not taken from a default company.",
                    challanNo);
                return;
            }

            var company = await _companyService.GetCompanyByIdAsync(companyId);
            var gst = CompanyGstResolver.Resolve(companyId, company.CompanyId, company.Gst_No);
            payload["gstNo"] = gst;
            payload["companyId"] = companyId;

            _logger.LogInformation(
                "Delivery challan PDF company resolved. ChallanNo={ChallanNo} CompanyId={CompanyId} CompanyName={CompanyName} Gst={MaskedGst}",
                challanNo,
                companyId,
                company.CompanyName,
                CompanyGstResolver.Mask(gst));
        }

        private static string ReadPayloadGst(JObject payload)
        {
            return (payload.Value<string>("gstNo") ?? payload.Value<string>("GstNo") ?? string.Empty).Trim();
        }
    }
}
