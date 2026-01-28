using System.Text.Json.Serialization;

namespace WimpeyTrack.Api.Dtos.Vision;

public class VisionReceiptDetectionResponse
{
    [JsonPropertyName("receipts")]
    public List<VisionBoundingBox> Receipts { get; set; } = [];
}