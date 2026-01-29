using System.IO.Compression;
using WimpeyTrack.Api.Dtos.ReportGeneration;

namespace WimpeyTrack.Api.Domain;

/// <summary>
/// Builds a zip containing the files generated from report.
/// </summary>
public interface IReportZipBuilder
{
    byte[] BuildZip(ReportArtifacts artifacts);
}

public class ReportZipBuilder : IReportZipBuilder
{
    public byte[] BuildZip(ReportArtifacts artifacts)
    {
        // This memory stream will contain the zip file
        using var zipStream = new MemoryStream();
        
        // Create the zip archive in memory
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            AddExpenseDocuments(archive, artifacts.ExpenseDocuments);
            AddReceiptPages(archive, artifacts.ReceiptPages);
        }
        
        return zipStream.ToArray();
    }

    private static void AddReceiptPages(ZipArchive archive, IReadOnlyList<ReceiptPage> pages)
    {
        foreach (var page in pages.OrderBy(p => p.PageIndex))
        {
            var entry = archive.CreateEntry(
                $"Receipts_Page_{page.PageIndex}.jpeg",
                CompressionLevel.Fastest);
            
            using var entryStream = entry.Open();
            entryStream.Write(page.ImageBytes);
        }
    }

    private static void AddExpenseDocuments(ZipArchive archive, IReadOnlyList<ExpenseDocument> documents)
    {
        foreach (var doc in documents)
        {
            var entry = archive.CreateEntry(
                $"Expenses_{doc.BookIndex}_Page_{doc.PageIndex}.pdf",
                CompressionLevel.Fastest);
            
            using var entryStream = entry.Open();
            entryStream.Write(doc.PdfBytes);
        }
    }
}