using System.Text.Json.Serialization;

namespace WimpeyTrack.Api.Dtos.Vision;

public class VisionBoundingBox
{
    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
    [JsonPropertyName("width")]
    public int Width { get; set; }
    [JsonPropertyName("height")]
    public int Height { get; set; }
}