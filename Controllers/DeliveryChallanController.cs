using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SSProjectSolution.Documents;
using SSProjectSolution.Request;
using System;
using System.IO;
using System.Linq;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryChallanController : ControllerBase
    {
        public DeliveryChallanController()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [HttpPost("GenerateAndDownloadDC")]
        public IActionResult GenerateAndDownloadDC([FromBody] JObject payload)
        {
            try
            {
                if (payload == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request payload" });
                }

                // Check EntryType to decide which document to render
                string entryType = payload.Value<string>("entryType") ?? "S";

                if (entryType == "M")
                {
                    // Meter-based flow (isolated)
                    var meterRequest = payload.ToObject<GenerateMeterDcRequest>();
                    if (meterRequest == null)
                    {
                        return BadRequest(new { success = false, message = "Failed to deserialize meter request" });
                    }

                    // Map common fields from JObject manually where property names differ
                    if (string.IsNullOrEmpty(meterRequest.CompanyName))
                    {
                        meterRequest.CompanyName = payload.Value<string>("receiverName") ?? string.Empty;
                    }
                    if (string.IsNullOrEmpty(meterRequest.Address))
                    {
                        meterRequest.Address = payload.Value<string>("receiverAddress") ?? string.Empty;
                    }
                    if (string.IsNullOrEmpty(meterRequest.GstNo))
                    {
                        var companyObj = payload.Value<JObject>("company");
                        if (companyObj != null)
                        {
                            meterRequest.GstNo = companyObj.Value<string>("gst") ?? string.Empty;
                        }
                    }
                    if (string.IsNullOrEmpty(meterRequest.Date))
                    {
                        meterRequest.Date = payload.Value<string>("date") ?? string.Empty;
                    }

                    // Map fields from JObject to fill specific fields (e.g. Design, Style, Color from items list if not top-level)
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

                    // Validate
                    if (meterRequest.Items == null || !meterRequest.Items.Any())
                    {
                        return BadRequest(new { success = false, message = "Meter details cannot be empty" });
                    }

                    var document = new MeterDeliveryChallanDocument(meterRequest);
                    using var stream = new MemoryStream();
                    document.GeneratePdf(stream);
                    var bytes = stream.ToArray();

                    string fileName = $"{meterRequest.DcNo}.pdf";
                    return File(bytes, "application/pdf", fileName);
                }
                else
                {
                    // Size-based flow (untouched, delegates to existing DeliveryChallanDocument)
                    var sizeRequest = payload.ToObject<GenerateDcRequest>();
                    if (sizeRequest == null)
                    {
                        return BadRequest(new { success = false, message = "Failed to deserialize size request" });
                    }

                    // Map common fields from JObject manually where property names differ
                    if (string.IsNullOrEmpty(sizeRequest.CompanyName))
                    {
                        sizeRequest.CompanyName = payload.Value<string>("receiverName") ?? string.Empty;
                    }
                    if (string.IsNullOrEmpty(sizeRequest.Address))
                    {
                        sizeRequest.Address = payload.Value<string>("receiverAddress") ?? string.Empty;
                    }
                    if (string.IsNullOrEmpty(sizeRequest.GstNo))
                    {
                        var companyObj = payload.Value<JObject>("company");
                        if (companyObj != null)
                        {
                            sizeRequest.GstNo = companyObj.Value<string>("gst") ?? string.Empty;
                        }
                    }
                    if (string.IsNullOrEmpty(sizeRequest.Date))
                    {
                        sizeRequest.Date = payload.Value<string>("date") ?? string.Empty;
                    }

                    // Map items
                    var itemsArray = payload.Value<JArray>("items");
                    if (itemsArray != null && itemsArray.Count > 0)
                    {
                        var firstItem = itemsArray[0] as JObject;
                        if (firstItem != null && string.IsNullOrEmpty(sizeRequest.JobReference))
                        {
                            string styleNo = firstItem.Value<string>("styleNo") ?? string.Empty;
                            string designName = firstItem.Value<string>("designName") ?? string.Empty;
                            string colour = firstItem.Value<string>("colour") ?? string.Empty;
                            sizeRequest.JobReference = $"{styleNo} - {designName} ({colour})";
                        }

                        sizeRequest.Items = itemsArray.Select(item => new DcItem
                        {
                            SrNo = 1,
                            Description = $"{item.Value<string>("styleNo")} - {item.Value<string>("designName")} ({item.Value<string>("colour")})",
                            Quantity = item.Value<decimal>("count"),
                            Uom = "Pcs"
                        }).ToList();
                    }

                    var document = new DeliveryChallanDocument(sizeRequest);
                    using var stream = new MemoryStream();
                    document.GeneratePdf(stream);
                    var bytes = stream.ToArray();

                    string fileName = $"{sizeRequest.DcNo}.pdf";
                    return File(bytes, "application/pdf", fileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to generate PDF: " + ex.Message });
            }
        }
    }
}
