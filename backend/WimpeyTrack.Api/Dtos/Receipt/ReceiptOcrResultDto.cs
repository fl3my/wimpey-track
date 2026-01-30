using WimpeyTrack.Api.Dtos.Shared;

namespace WimpeyTrack.Api.Dtos.Receipt;

public class ReceiptOcrResultDto
{
    public string ImageBase64 { get; set; } = null!;
    public List<ReceiptData> ReceiptData { get; set; } = [];
}