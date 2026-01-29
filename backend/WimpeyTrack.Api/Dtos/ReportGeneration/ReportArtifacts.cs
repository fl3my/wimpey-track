namespace WimpeyTrack.Api.Dtos.ReportGeneration;

public class ReportArtifacts
{
    public Guid ReportId { get; set; }
    public IReadOnlyList<ExpenseDocument> ExpenseDocuments { get; set; } = [];
    public IReadOnlyList<ReceiptPage> ReceiptPages { get; set; } = [];
}