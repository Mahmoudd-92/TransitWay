using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TransitWay.Data;
using TransitWay.Dtos;
using System.Net.Http.Headers;

[ApiController]
[Route("api/complaints")]
public class ComplaintController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _env;

    private const string AiBaseUrl = "http://54.91.157.86:8000";

    public ComplaintController(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment env)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _env = env;
    }

    [HttpPost("report")]
    public async Task<IActionResult> UploadComplaint([FromForm] ReportImageDto dto)
    {
        if (dto.Image == null || dto.Image.Length == 0)
            return BadRequest("No image uploaded");

        var client = _httpClientFactory.CreateClient();

        using var content = new MultipartFormDataContent();
        using var stream = dto.Image.OpenReadStream();

        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(dto.Image.ContentType ?? "image/jpeg");

        content.Add(fileContent, "file", dto.Image.FileName);

        var aiResponse = await client.PostAsync(
            $"{AiBaseUrl}/predict-upload/",
            content);

        var json = await aiResponse.Content.ReadAsStringAsync();

        if (!aiResponse.IsSuccessStatusCode)
        {
            return StatusCode(500, $"AI Error: {json}");
        }

        using var doc = JsonDocument.Parse(json);

        bool problemDetected = false;
        string imageUrl = "";

        if (doc.RootElement.TryGetProperty("predictions", out var predictionsArray)
            && predictionsArray.GetArrayLength() > 0)
        {
            var firstPrediction = predictionsArray[0];

            var className = firstPrediction
                .GetProperty("class_name")
                .GetString();

            problemDetected = !string.IsNullOrEmpty(className);

           
        }

        if (doc.RootElement.TryGetProperty("output_image", out var outputImageElement))
        {
            imageUrl = outputImageElement.GetString();
        }
        else
        {
            return StatusCode(500, $"Invalid AI response: {json}");
        }

        string fullImageUrl = imageUrl;

        var originalFolder = Path.Combine(_env.WebRootPath, "uploads", "originals");
        var resultFolder = Path.Combine(_env.WebRootPath, "uploads", "results");

        Directory.CreateDirectory(originalFolder);
        Directory.CreateDirectory(resultFolder);

        var originalFileName = $"original_{Guid.NewGuid()}.jpg";
        var originalPath = Path.Combine(originalFolder, originalFileName);

        using (var fileStream = new FileStream(originalPath, FileMode.Create))
        {
            await dto.Image.CopyToAsync(fileStream);
        }

        var imageBytes = await client.GetByteArrayAsync(fullImageUrl);

        var resultFileName = $"result_{Guid.NewGuid()}.jpg";
        var resultPath = Path.Combine(resultFolder, resultFileName);

        await System.IO.File.WriteAllBytesAsync(resultPath, imageBytes);

        var complaint = new Complaint
        {
            BusId = dto.BusId,
            UserId = dto.UserId,
            OriginalImagePath = $"/uploads/originals/{originalFileName}",
            ResultImagePath = $"/uploads/results/{resultFileName}",
            ProblemDetected = problemDetected
        };

        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Complaint analyzed successfully",
            problemDetected,

            originalImage =
                $"{Request.Scheme}://{Request.Host}/uploads/originals/{originalFileName}",

            resultImage =
                $"{Request.Scheme}://{Request.Host}/uploads/results/{resultFileName}"
        });
    }
}