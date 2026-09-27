using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using SSProjectSolution.Documents;
using SSProjectSolution.Exceptions;
using SSProjectSolution.Models;
using SSProjectSolution.Repositories;
using SSProjectSolution.Request;
using SSProjectSolution.Response;
using SSProjectSolution.Settings;
using SSProjectSolution.Utilities;

namespace SSProjectSolution.Services
{
    public class AdvanceChallanService : IAdvanceChallanService
    {
        private readonly ChallanSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly IAdvanceAmountRepository _repository;
        private readonly ILogger<AdvanceChallanService> _logger;

        public AdvanceChallanService(
            IOptions<ChallanSettings> options,
            IConfiguration configuration,
            IAdvanceAmountRepository repository,
            ILogger<AdvanceChallanService> logger)
        {
            _settings = options.Value;
            _configuration = configuration;
            _repository = repository;
            _logger = logger;
        }

        public async Task<(string ChallanNo, string ChallanFileName)> GenerateAndSaveChallanAsync(AdvanceAmount advance)
        {
            if (advance == null || advance.Id <= 0)
                throw new AdvanceChallanValidationException("Advance amount record not found.");

            var challanNo = await ResolveChallanNoAsync(advance.Id, advance.Date, advance.ChallanNo);
            var pdf = BuildPdf(advance.Name, advance.Date, advance.Amount, advance.Remarks, challanNo, advance.CreatedBy);
            var fileName = BuildFileName(challanNo);
            await SavePdfAsync(advance.Id, challanNo, fileName, pdf);
            return (challanNo, fileName);
        }

        public async Task<AdvanceChallanFileResult> GenerateChallanAsync(GenerateAdvanceChallanRequest request, string generatedBy)
        {
            if (request == null || request.AdvanceId <= 0)
                throw new AdvanceChallanValidationException("AdvanceId is required.");

            var record = await _repository.GetAdvanceByIdAsync(request.AdvanceId);
            if (record == null)
                throw new AdvanceAmountNotFoundException();

            var name = FirstNonEmpty(request.Name, record.Name);
            var date = request.Date == default ? record.Date : request.Date;
            var amount = request.Amount > 0 ? request.Amount : record.Amount;
            var remarks = request.Remarks ?? record.Remarks;

            if (string.IsNullOrWhiteSpace(name))
                throw new AdvanceChallanValidationException("Name is required.");
            if (date == default)
                throw new AdvanceChallanValidationException("Date is required.");
            if (amount <= 0)
                throw new AdvanceChallanValidationException("Amount must be a positive value.");

            var challanNo = await ResolveChallanNoAsync(record.Id, date, record.ChallanNo);
            var pdf = BuildPdf(name.Trim(), date, amount, remarks, challanNo, generatedBy);
            var fileName = BuildFileName(challanNo);
            await SavePdfAsync(record.Id, challanNo, fileName, pdf);

            return new AdvanceChallanFileResult
            {
                PdfBytes = pdf,
                FileName = fileName,
                ChallanNo = challanNo
            };
        }

        private async Task<string> ResolveChallanNoAsync(int advanceId, DateTime advanceDate, string? existingChallanNo)
        {
            if (!string.IsNullOrWhiteSpace(existingChallanNo))
                return existingChallanNo.Trim();

            try
            {
                return await _repository.AllocateNextChallanNoAsync(advanceId, advanceDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to allocate a sequential challan number for Advance ID {AdvanceId}. Using identity fallback.", advanceId);
                var year = advanceDate.Year >= 2000 ? advanceDate.Year : DateTime.Now.Year;
                return $"AC-{year}-{advanceId:D3}";
            }
        }

        private byte[] BuildPdf(string name, DateTime date, decimal amount, string? remarks, string challanNo, string? generatedBy)
        {
            var company = ReadCompany();
            var data = new AdvanceChallanPdfData
            {
                CompanyName = company.Name,
                Address = company.Address,
                AddressLines = company.AddressLines,
                Phone = company.Phone,
                GstNumber = company.GstNumber,
                LogoBytes = company.LogoBytes,
                ChallanNo = challanNo,
                Name = name,
                Date = date,
                Amount = amount,
                AmountFormatted = amount.ToString("#,##0.00", CultureInfo.InvariantCulture),
                AmountInWords = NumberToWordsConverter.ConvertToWords(amount),
                Remarks = string.IsNullOrWhiteSpace(remarks) ? "-" : remarks.Trim(),
                GeneratedBy = string.IsNullOrWhiteSpace(generatedBy) ? "Admin" : generatedBy.Trim()
            };

            return new AdvancePaymentChallanDocument(data).GeneratePdf();
        }

        private async Task SavePdfAsync(int advanceId, string challanNo, string fileName, byte[] pdfBytes)
        {
            try
            {
                var storagePath = _settings.StoragePath;
                if (string.IsNullOrWhiteSpace(storagePath))
                {
                    _logger.LogWarning("ChallanSettings:StoragePath is not configured. Using the local AdvanceChallans folder.");
                    storagePath = Path.Combine(Directory.GetCurrentDirectory(), "AdvanceChallans");
                }

                Directory.CreateDirectory(storagePath);
                var fullPath = Path.Combine(storagePath, fileName);
                await File.WriteAllBytesAsync(fullPath, pdfBytes);
                await _repository.UpdateAdvanceChallanAsync(advanceId, challanNo, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Advance challan {ChallanNo} was generated but could not be stored for Advance ID {AdvanceId}.", challanNo, advanceId);
            }
        }

        private static string BuildFileName(string challanNo)
        {
            var safe = challanNo;
            foreach (var invalid in Path.GetInvalidFileNameChars())
                safe = safe.Replace(invalid, '-');
            return $"Advance_Challan_{safe}.pdf";
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        private (string Name, string Address, string[] AddressLines, string Phone, string GstNumber, byte[]? LogoBytes) ReadCompany()
        {
            var name = FirstNonEmpty(
                _configuration["CompanySettings:CompanyName"],
                _configuration["CompanySettings:Name"]);
            if (string.IsNullOrWhiteSpace(name))
                name = "S.S. EMBROIDERY";

            var addressLines = new[]
            {
                _configuration["CompanySettings:AddressLine1"],
                _configuration["CompanySettings:AddressLine2"],
                _configuration["CompanySettings:AddressLine3"]
            }.Where(part => !string.IsNullOrWhiteSpace(part))
             .Select(part => Regex.Replace(Regex.Replace(part!.Trim(), @"\s+", " "), @",(?=\S)", ", ").Trim().TrimEnd(','))
             .Where(part => part.Length > 0)
             .ToArray();

            var address = _configuration["CompanySettings:Address"];
            if (addressLines.Length == 0 && !string.IsNullOrWhiteSpace(address))
            {
                address = Regex.Replace(address, @"\s+", " ").Replace(" ,", ",").Trim();
                addressLines = new[] { address };
            }
            else
            {
                address = string.Join(", ", addressLines);
            }

            var phone = _configuration["CompanySettings:PhoneNumber"]?.Trim();
            var alternate = _configuration["CompanySettings:AlternatePhoneNumber"]?.Trim();
            if (!string.IsNullOrWhiteSpace(phone) && !string.IsNullOrWhiteSpace(alternate))
                phone = $"{phone} | {alternate}";
            else if (string.IsNullOrWhiteSpace(phone))
                phone = alternate;

            var gst = FirstNonEmpty(
                _configuration["CompanySettings:GSTNumber"],
                _configuration["CompanySettings:GstNo"]);

            return (name, address, addressLines, phone ?? string.Empty, gst, TryLoadLogo());
        }

        private byte[]? TryLoadLogo()
        {
            var configured = _configuration["CompanySettings:LogoPath"];
            var candidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(configured))
            {
                if (Path.IsPathRooted(configured))
                    candidates.Add(configured);
                else
                {
                    candidates.Add(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configured)));
                    candidates.Add(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configured)));
                }
            }

            candidates.Add(Path.Combine(AppContext.BaseDirectory, "Assets", "logo.jpeg"));
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "Assets", "logo.jpeg"));

            foreach (var path in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    if (File.Exists(path))
                        return File.ReadAllBytes(path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to read the company logo from {LogoPath}.", path);
                }
            }

            _logger.LogWarning("Company logo was not found. PDF will be generated without the logo. Checked: {Paths}", string.Join("; ", candidates));
            return null;
        }
    }
}
