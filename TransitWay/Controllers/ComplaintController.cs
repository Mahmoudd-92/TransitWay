using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TransitWay.Data;
using TransitWay.Dtos;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/complaints")]
public class ComplaintController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _env;

    private const string AiBaseUrl = "http://54.91.157.86:8000";

    private static string MapToCategory(string? className)
    {
        if (string.IsNullOrWhiteSpace(className))
            return "Unknown";

        return className.ToLowerInvariant() switch
        {
            "damaged_window" => "Damaged Window",
            "damaged_seat" => "Damaged Seat",
            "cigarettes" => "Smoking",
            "smoke" => "Smoking",
            "drink" => "Drink",
            var t when t.StartsWith("trash") => "Trash",
            _ => "Other"
        };
    }

    public ComplaintController(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment env)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _env = env;
    }


    [HttpGet]
    public async Task<IActionResult> GetAllComplaints()
    {
        var complaints = await _context.Complaints
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.BusId,
                c.UserId,
                c.ProblemDetected,
                c.Category,
                c.TextComplaint,
                c.CreatedAt,
                originalImage = string.IsNullOrEmpty(c.OriginalImagePath)
                    ? null
                    : $"{Request.Scheme}://{Request.Host}{c.OriginalImagePath}",
                resultImage = string.IsNullOrEmpty(c.ResultImagePath)
                    ? null
                    : $"{Request.Scheme}://{Request.Host}{c.ResultImagePath}"
            })
            .ToListAsync();

        return Ok(complaints);
    }

   
    [HttpPost("report")]
    public async Task<IActionResult> UploadComplaint([FromForm] ReportImageDto dto)
    {
        bool hasImage = dto.Image != null && dto.Image.Length > 0;
        bool hasText = !string.IsNullOrWhiteSpace(dto.TextComplaint);

        if (!hasImage && !hasText)
            return BadRequest("send photo or text");

        var client = _httpClientFactory.CreateClient();

        bool problemDetected = false;
        string category = "Unknown";
        string originalImagePath = "";
        string resultImagePath = "";
        string originalImageUrl = "";
        string resultImageUrl = "";

        if (hasImage)
        {
            using var content = new MultipartFormDataContent();
            using var stream = dto.Image!.OpenReadStream();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(dto.Image.ContentType ?? "image/jpeg");

            content.Add(fileContent, "file", dto.Image.FileName);

            var aiResponse = await client.PostAsync(
                $"{AiBaseUrl}/predict-upload/",
                content);

            var json = await aiResponse.Content.ReadAsStringAsync();

            if (!aiResponse.IsSuccessStatusCode)
                return StatusCode(500, $"AI Error: {json}");

            using var doc = JsonDocument.Parse(json);

            string? detectedClass = null;

            if (doc.RootElement.TryGetProperty("predictions", out var predictionsArray)
                && predictionsArray.GetArrayLength() > 0)
            {
                var firstPrediction = predictionsArray[0];
                detectedClass = firstPrediction
                    .GetProperty("class_name")
                    .GetString();

                problemDetected = !string.IsNullOrEmpty(detectedClass);
            }

            category = MapToCategory(detectedClass);

            if (!doc.RootElement.TryGetProperty("output_image", out var outputImageElement))
                return StatusCode(500, $"Invalid AI response — missing output_image: {json}");

            string fullImageUrl = outputImageElement.GetString() ?? "";

            var originalFolder = Path.Combine(_env.WebRootPath, "uploads", "originals");
            Directory.CreateDirectory(originalFolder);

            var originalFileName = $"original_{Guid.NewGuid()}.jpg";
            var originalFilePath = Path.Combine(originalFolder, originalFileName);

            using (var fileStream = new FileStream(originalFilePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(fileStream);
            }

            var imageBytes = await client.GetByteArrayAsync(fullImageUrl);
            var resultFolder = Path.Combine(_env.WebRootPath, "uploads", "results");
            Directory.CreateDirectory(resultFolder);

            var resultFileName = $"result_{Guid.NewGuid()}.jpg";
            var resultFilePath = Path.Combine(resultFolder, resultFileName);

            await System.IO.File.WriteAllBytesAsync(resultFilePath, imageBytes);

            originalImagePath = $"/uploads/originals/{originalFileName}";
            resultImagePath = $"/uploads/results/{resultFileName}";
            originalImageUrl = $"{Request.Scheme}://{Request.Host}/uploads/originals/{originalFileName}";
            resultImageUrl = $"{Request.Scheme}://{Request.Host}/uploads/results/{resultFileName}";
        }

        if (!hasImage && hasText)
        {
            problemDetected = true;
            category = "Text Report";
        }

        var complaint = new Complaint
        {
            BusId = dto.BusId,
            UserId = dto.UserId,
            OriginalImagePath = originalImagePath,
            ResultImagePath = resultImagePath,
            ProblemDetected = problemDetected,
            Category = category,
            TextComplaint = dto.TextComplaint?.Trim()
        };

        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "report send successfully",
            problemDetected,
            category,
            textComplaint = dto.TextComplaint,
            originalImage = string.IsNullOrEmpty(originalImageUrl) ? null : originalImageUrl,
            resultImage = string.IsNullOrEmpty(resultImageUrl) ? null : resultImageUrl
        });
    }
}