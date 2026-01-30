using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using WimpeyTrack.Api.Domain;
using WimpeyTrack.Api.Dtos.Book;
using WimpeyTrack.Api.Dtos.ReportGeneration;

namespace WimpeyTrack.Api.Services;

/// <summary>
/// Responsible for congregating all services related to PDF generation to return a objecc
/// containing all report PDF's and receipts for a date range.
/// </summary>
public interface IReportGenerationService
{
    Task<ReportArtifacts?> GenerateAsync(DateOnly startDate, DateOnly endDate);
}

public class ReportGenerationService : IReportGenerationService
{
    private readonly IBookBuilder _bookBuilder;
    private readonly IExpenseWorkbookBuilder _workbookBuilder;
    private readonly IPdfConverterService _pdfConverter;
    private readonly IReceiptProvider _receiptProvider;
    private readonly IImageProcessingService _imageProcessing;

    public ReportGenerationService(IExpenseWorkbookBuilder workbookBuilder, IBookBuilder bookBuilder, IPdfConverterService pdfConverter, IImageProcessingService imageProcessing, IReceiptProvider receiptProvider)
    {
        _workbookBuilder = workbookBuilder;
        _bookBuilder = bookBuilder;
        _pdfConverter = pdfConverter;
        _imageProcessing = imageProcessing;
        _receiptProvider = receiptProvider;
    }

    public async Task<ReportArtifacts?> GenerateAsync(DateOnly startDate, DateOnly endDate)
    {
        var books = await _bookBuilder.BuildAsync(startDate, endDate);
        
        if (books.Count == 0) return null;
        
        var expenseDocs = await GenerateExpenseDocumentsAsync(books, startDate, endDate);
        var receiptPages = await GenerateReceiptPagesAsync(startDate, endDate);
        
        return new ReportArtifacts()
        {
            ReportId = Guid.NewGuid(),
            ExpenseDocuments = expenseDocs,
            ReceiptPages = receiptPages,
        };
    }

    private async Task<IReadOnlyList<ReceiptPage>> GenerateReceiptPagesAsync(DateOnly startDate, DateOnly endDate)
    {
        const int receiptsPerPage = 4;
        var pages = new List<ReceiptPage>();
        
        // get receipts
        var receipts = await _receiptProvider.GetAsync(startDate, endDate);
        
        // Get the total number of image pages
        var totalPages = (int)Math.Ceiling(receipts.Count / (double)receiptsPerPage);

        for (var i = 0; i < totalPages; i++)
        {
            var images = receipts
                .Skip(i * receiptsPerPage)
                .Take(receiptsPerPage)
                .Select(r => r.ImageBytes)
                .ToList();
            
            if (images.Count == 0)
                continue;

            var combined = await _imageProcessing.CombineReceiptsAsync(images, receiptsPerPage);
            
            pages.Add(new ReceiptPage()
            {
                PageIndex = i + 1,
                ImageBytes = combined
            });
        }

        return pages;
    }

    private async Task<IReadOnlyList<ExpenseDocument>> GenerateExpenseDocumentsAsync(IReadOnlyList<Book> books, DateOnly startDate, DateOnly endDate)
    {
        var result = new List<ExpenseDocument>();
        
        // For each book
        for (var i = 0; i < books.Count; i++)
        {
            // Build a workbook
            await using var workbook = _workbookBuilder.Build(books[i]);
            var pageRange = GetPageRange(books[i]);
            
            // Convert to PDF
            var pdfBytes = await _pdfConverter.ConvertXlsxToPdfAsync(workbook, pageRange);
            
            using var input = new MemoryStream(pdfBytes);
            var pdf = PdfReader.Open(input, PdfDocumentOpenMode.Import);

            // Convert the pdf into single pages
            for (var p = 0; p < pdf.PageCount; p++)
            {
                using var single = new PdfDocument();

                single.AddPage(pdf.Pages[p]);
                
                using var stream = new MemoryStream();
                
                await single.SaveAsync(stream);

                result.Add(new ExpenseDocument()
                {
                    BookIndex = i + 1,
                    PageIndex = p + 1,
                    PdfBytes = stream.ToArray()
                });
            }
        }

        return result;
    }

    private static string GetPageRange(Book book)
    {
        // Base pages always included
        var pages = new List<int> { 1, 3 };

        // If mileage spills onto a second sheet, include it
        if (book.Sheets.Count > 1)
        {
            pages.Add(4);
        }

        return string.Join(",", pages);
    }
}