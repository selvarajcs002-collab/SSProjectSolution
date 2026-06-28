using Microsoft.Extensions.Configuration;
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

        public PdfGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<byte[]> GeneratePdfAsync(JObject payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload), "Invalid request payload");

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

            if (string.IsNullOrEmpty(meterRequest.GstNo))
            {
                var companyObj = payload.Value<JObject>("company");
                if (companyObj != null)
                {
                    meterRequest.GstNo = companyObj.Value<string>("gst") ?? string.Empty;
                }
            }

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

            if (string.IsNullOrEmpty(sizeRequest.GstNo))
            {
                var directGst = payload.Value<string>("gstNo");
                if (!string.IsNullOrWhiteSpace(directGst))
                {
                    sizeRequest.GstNo = directGst;
                }
                else
                {
                    var companyObj = payload.Value<JObject>("company");
                    if (companyObj != null)
                    {
                        sizeRequest.GstNo = companyObj.Value<string>("gst") ?? string.Empty;
                    }
                }
            }

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
    }
}
