using WimpeyTrack.Api.Enums;

namespace WimpeyTrack.Api.Dtos;

public class ReceiptOcrResultDto
{
    public string ImageBase64 { get; set; } = null!;
    public List<ReceiptData> ReceiptData { get; set; } = [];
}