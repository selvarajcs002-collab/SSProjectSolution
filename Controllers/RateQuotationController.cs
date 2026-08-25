using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Response;
using SSProjectSolution.Services;
using SSProjectSolution.Documents;
using System.IO;
using System.IO;
using QuestPDF.Fluent;
using Microsoft.Extensions.Configuration;

namespace SSProjectSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Uncomment when authorization is configured
    public class RateQuotationController : ControllerBase
    {
        private readonly IRateQuotationService _service;
        private readonly string ImageFolderPath;

        public RateQuotationController(IRateQuotationService service, IConfiguration configuration)
        {
            _service = service;
            ImageFolderPath = configuration["RateQuotationSettings:ImageFolderPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "RateQuotationImages");
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<long>>> CreateAsync([FromBody] RateQuotationCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<long>.ErrorResponse("Validation Failed", errors));
            }

            var response = await _service.CreateAsync(createDto);

            if (response.Success)
            {
                return CreatedAtAction(nameof(GetByIdAsync).Replace("Async", ""), new { id = response.Data }, response);
            }

            // You can map to specific status codes based on response.Message if needed
            if (response.Message.Contains("already exists"))
            {
                return Conflict(response);
            }

            return BadRequest(response);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(long id, [FromBody] RateQuotationUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.ErrorResponse("Validation Failed", errors));
            }

            var response = await _service.UpdateAsync(id, updateDto);

            if (response.Success)
            {
                return Ok(response);
            }

            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(long id, [FromQuery] long modifiedBy)
        {
            var response = await _service.DeleteAsync(id, modifiedBy);

            if (response.Success)
            {
                return Ok(response);
            }

            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<ActionResult<ApiResponse<RateQuotationResponseDto>>> GetByIdAsync(long id)
        {
            var response = await _service.GetByIdAsync(id);

            if (response.Success)
            {
                return Ok(response);
            }

            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        [HttpGet("getall")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RateQuotationResponseDto>>>> GetAllAsync()
        {
            var response = await _service.GetAllAsync();

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RateQuotationResponseDto>>>> SearchAsync([FromBody] RateQuotationSearchDto searchDto)
        {
            var response = await _service.SearchAsync(searchDto);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<IEnumerable<RateQuotationResponseDto>>>>> GetPagedAsync([FromBody] RateQuotationSearchDto searchDto)
        {
            var response = await _service.GetPagedAsync(searchDto);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }



        [HttpPost("upload-image/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> UploadImageAsync(long id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("No image file was provided."));
            }

            try
            {
                if (!Directory.Exists(ImageFolderPath))
                {
                    Directory.CreateDirectory(ImageFolderPath);
                }

                // Delete older image files for this rate quotation ID to override with the new image
                var existingFiles = Directory.GetFiles(ImageFolderPath, $"{id}.*");
                foreach (var existingFile in existingFiles)
                {
                    try
                    {
                        System.IO.File.Delete(existingFile);
                    }
                    catch
                    {
                        // Ignore lock errors
                    }
                }

                var extension = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = ".jpg";
                }

                var newFilePath = Path.Combine(ImageFolderPath, $"{id}{extension}");
                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/api/RateQuotation/image/{id}";
                return Ok(ApiResponse<string>.SuccessResponse(imageUrl, "Image uploaded successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Error uploading image: " + ex.Message));
            }
        }

        public class Base64ImageRequest
        {
            public string Base64Image { get; set; } = string.Empty;
        }

        [HttpPost("upload-image-base64/{id}")]
        public async Task<ActionResult<ApiResponse<string>>> UploadBase64ImageAsync(long id, [FromBody] Base64ImageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Base64Image))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("No base64 image content provided."));
            }

            try
            {
                if (!Directory.Exists(ImageFolderPath))
                {
                    Directory.CreateDirectory(ImageFolderPath);
                }

                // Delete older image files for this rate quotation ID to override with the new image
                var existingFiles = Directory.GetFiles(ImageFolderPath, $"{id}.*");
                foreach (var existingFile in existingFiles)
                {
                    try
                    {
                        System.IO.File.Delete(existingFile);
                    }
                    catch
                    {
                        // Ignore lock errors
                    }
                }

                string extension = ".jpg";
                string base64Data = request.Base64Image;

                if (base64Data.Contains(","))
                {
                    var parts = base64Data.Split(',');
                    var header = parts[0];
                    base64Data = parts[1];

                    if (header.Contains("png")) extension = ".png";
                    else if (header.Contains("gif")) extension = ".gif";
                    else if (header.Contains("webp")) extension = ".webp";
                }

                byte[] bytes = Convert.FromBase64String(base64Data);
                var newFilePath = Path.Combine(ImageFolderPath, $"{id}{extension}");
                await System.IO.File.WriteAllBytesAsync(newFilePath, bytes);

                var imageUrl = $"/api/RateQuotation/image/{id}";
                return Ok(ApiResponse<string>.SuccessResponse(imageUrl, "Image uploaded successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Error saving base64 image: " + ex.Message));
            }
        }

        [HttpGet("image/{id}")]
        [AllowAnonymous]
        public IActionResult GetImage(long id)
        {
            try
            {
                if (!Directory.Exists(ImageFolderPath))
                {
                    return NotFound("Image directory not found.");
                }

                var files = Directory.GetFiles(ImageFolderPath, $"{id}.*");
                if (files.Length == 0)
                {
                    return NotFound("Image not found.");
                }

                var filePath = files[0];
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    ".bmp" => "image/bmp",
                    _ => "application/octet-stream"
                };

                return PhysicalFile(filePath, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error retrieving image: " + ex.Message);
            }
        }

        [HttpDelete("image/{id}")]
        public IActionResult DeleteImage(long id)
        {
            try
            {
                if (Directory.Exists(ImageFolderPath))
                {
                    var files = Directory.GetFiles(ImageFolderPath, $"{id}.*");
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Image deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Error deleting image: " + ex.Message));
            }
        }

        [HttpGet("pdf/{id}")]
        public async Task<IActionResult> GeneratePdfAsync(long id)
        {
            var response = await _service.GetByIdAsync(id);
            if (!response.Success || response.Data == null)
            {
                return NotFound(response.Message);
            }

            var model = response.Data;
            string imagePath = string.Empty;

            if (Directory.Exists(ImageFolderPath))
            {
                var files = Directory.GetFiles(ImageFolderPath, $"{id}.*");
                if (files.Length > 0)
                {
                    imagePath = files[0];
                }
            }

            var document = new RateQuotationDocument(model, imagePath);
            var pdfBytes = document.GeneratePdf();

            var fileName = $"RateQuotation_{model.QuotationNo}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        public class RateQuotationPdfRequest
        {
            public long QuotationId { get; set; }
        }

        [HttpPost("download-pdf")]
        public async Task<IActionResult> DownloadPdf([FromBody] RateQuotationPdfRequest request)
        {
            if (request == null || request.QuotationId <= 0)
            {
                return BadRequest("Invalid quotation ID.");
            }

            try
            {
                var response = await _service.GetByIdAsync(request.QuotationId);
                if (!response.Success || response.Data == null)
                {
                    return NotFound(response.Message);
                }

                var model = response.Data;
                string imagePath = string.Empty;

                if (Directory.Exists(ImageFolderPath))
                {
                    var files = Directory.GetFiles(ImageFolderPath, $"{request.QuotationId}.*");
                    if (files.Length > 0)
                    {
                        imagePath = files[0];
                    }
                }

                var document = new RateQuotationDocument(model, imagePath);
                var pdfBytes = document.GeneratePdf();

                var fileName = $"RateQuotation_{model.QuotationNo}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                // Note: Consider logging the exception here using an injected ILogger
                return StatusCode(500, "Error generating PDF: " + ex.Message);
            }
        }
    }
}
