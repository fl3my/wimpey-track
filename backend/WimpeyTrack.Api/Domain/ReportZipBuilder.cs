using System.IO.Compression;
using WimpeyTrack.Api.Dtos.ReportGeneration;

namespace WimpeyTrack.Api.Domain;

/// <summary>
/// Builds a zip containing the files generated from report.
/// </summary>
public interface IReportZipBuilder
{
    byte[] BuildZip(ReportResult report, DateOnly startDate, DateOnly endDate);
}

public class ReportZipBuilder : IReportZipBuilder
{
    public byte[] BuildZip(ReportResult report, DateOnly startDate, DateOnly endDate)
    {
        // This memory stream will contain the zip file
        using var zipStream = new MemoryStream();
        
        // Create the zip archive in memory
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            // Add the PDF files
            foreach (var doc in report.ExpenseDocuments)
            {
                var entryName =
                    $"Expenses_{doc.BookIndex}_Page_{doc.PageIndex}_" +
                    $"{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.pdf";
                
                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                
                using var entryStream = entry.Open();
                entryStream.Write(doc.PdfBytes);
            }
            
            // Add the receipt images
            foreach (var receipt in report.ReceiptPages)
            {
                var entryName = $"Receipts_Page_{receipt.PageIndex}.jpeg";
                
                var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
                
                using var entryStream = entry.Open();
                entryStream.Write(receipt.ImageBytes);
            }
        }
        return zipStream.ToArray();
    }
}