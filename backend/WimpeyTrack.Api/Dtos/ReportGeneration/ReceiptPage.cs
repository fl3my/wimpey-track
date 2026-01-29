namespace WimpeyTrack.Api.Dtos.ReportGeneration;

public class ReceiptPage
{
    public int PageIndex { get; init; }
    public byte[] ImageBytes { get; init; } = Array.Empty<byte>();
}