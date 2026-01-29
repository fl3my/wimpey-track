using WimpeyTrack.Api.Dtos.ReportGeneration;

namespace WimpeyTrack.Api.Mappings;

public static class ReportFileMappings
{
    public static ReportFile ToReportFile(this ExpenseDocument doc)
        => new()
        {
            FileName = $"Expenses_Book_{doc.BookIndex}_Page_{doc.PageIndex + 1}.pdf",
            Content = doc.PdfBytes,
            ContentType = "application/pdf"
        };

    public static ReportFile ToReportFile(this ReceiptPage page)
        => new()
        {
            FileName = $"Receipts_Page_{page.PageIndex}.jpg",
            Content = page.ImageBytes,
            ContentType = "image/jpeg"
        };
}