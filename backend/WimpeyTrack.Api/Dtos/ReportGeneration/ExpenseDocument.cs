namespace WimpeyTrack.Api.Dtos.ReportGeneration;

public class ExpenseDocument
{
    public int BookIndex { get; init; }
    public int PageIndex { get; init; }
    public byte[] PdfBytes { get; init; } = Array.Empty<byte>();
}