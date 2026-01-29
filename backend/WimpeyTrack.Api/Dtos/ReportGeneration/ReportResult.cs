namespace WimpeyTrack.Api.Dtos.ReportGeneration;

public class ReportResult
{
    public IReadOnlyList<ExpenseDocument> ExpenseDocuments { get; init; } = new List<ExpenseDocument>();
    public IReadOnlyList<ReceiptPage> ReceiptPages { get; init; } = new List<ReceiptPage>();
}