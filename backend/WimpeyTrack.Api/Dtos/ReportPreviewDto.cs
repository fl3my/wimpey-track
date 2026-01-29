namespace WimpeyTrack.Api.Dtos;

public class ReportPreviewDto
{
    public Guid ReportId { get; set; }

    public IReadOnlyList<FileLinkDto> ExpenseDocuments { get; init; } = [];
    public IReadOnlyList<FileLinkDto> ReceiptPages { get; init; } = [];
}

public class FileLinkDto
{
    public string FileName {get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    
}