using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SSProjectSolution.Request;

namespace SSProjectSolution.Services
{
    public class PrintService : IPrintService
    {
        private readonly IConfiguration _configuration;

        public PrintService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SavePdfAsync(PrintPdfRequest request)
        {
            if (string.IsNullOrEmpty(request.Base64Pdf))
                throw new ArgumentException("PDF data is missing.");

            string path = _configuration["PdfSettings:SavePath"] ?? @"C:\Users\ADMIN\Desktop\DC";
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // companyname(first three char) + dcNo
            string compName = string.IsNullOrEmpty(request.CompanyName) ? "UNK" : 
                (request.CompanyName.Length >= 3 ? request.CompanyName.Substring(0, 3) : request.CompanyName);
            
            string fileName = $"{compName}_{request.DcNo}.pdf";
            
            // Clean filename to prevent invalid chars
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

            string fullPath = Path.Combine(path, fileName);

            string base64Data = request.Base64Pdf.Contains(",") ? request.Base64Pdf.Split(',')[1] : request.Base64Pdf;
            byte[] bytes = Convert.FromBase64String(base64Data);

            await File.WriteAllBytesAsync(fullPath, bytes);

            return fullPath;
        }
    }
}
