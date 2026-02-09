using System.Net.Http.Headers;
using System.Text.Json;
using WimpeyTrack.Api.Dtos.Vision;

namespace WimpeyTrack.Api.Services;

public interface IVisionService
{
    Task<VisionReceiptDetectionResponse> DetectReceiptsAsync(
        IFormFile file,
        CancellationToken cancellationToken = default
    );
}

public class VisionService : IVisionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VisionService> _logger;
    
    public VisionService(HttpClient httpClient, ILogger<VisionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<VisionReceiptDetectionResponse> DetectReceiptsAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty");

        using var content = new MultipartFormDataContent();

        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, "file", file.FileName);

        using var response = await _httpClient.PostAsync(
            "v1/receipts/detect",
            content,
            cancellationToken
            );
        
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStreamAsync(cancellationToken);

        return JsonSerializer.Deserialize<VisionReceiptDetectionResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        )!;
    }
}