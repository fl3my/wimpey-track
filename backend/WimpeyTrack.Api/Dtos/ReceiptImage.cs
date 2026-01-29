namespace WimpeyTrack.Api.Dtos;

public sealed class ReceiptImage
{
    public DateOnly Date { get; init; }
    public byte[] ImageBytes { get; init; } = Array.Empty<byte>();
}