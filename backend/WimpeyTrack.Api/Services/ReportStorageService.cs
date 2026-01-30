using WimpeyTrack.Api.Dtos.ReportGeneration;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Services;

public interface IReportStorageService
{
    Task<string> SaveAsync (Guid reportId, ReportArtifacts artifacts);
    Task DeleteAsync(Guid reportId);
    Task<bool> ExistsAsync(Guid reportId);
    IReadOnlyList<string> GetExpenseFiles(Guid reportId);
    IReadOnlyList<string> GetReceiptFiles(Guid reportId);
}

public class ReportStorageService  : IReportStorageService
{
    private readonly string _wwwroot;
    
    public ReportStorageService(IWebHostEnvironment env)
    {
        _wwwroot = env.WebRootPath;
    }
    public async Task<string> SaveAsync(Guid reportId, ReportArtifacts artifacts)
    {
        var reportRoot = Path.Combine(_wwwroot, "reports", reportId.ToString());
        
        var expenseFolder = Path.Combine(reportRoot, "expenses");
        var receiptFolder = Path.Combine(reportRoot, "receipts");
        
        // Create directories
        Directory.CreateDirectory(expenseFolder);
        Directory.CreateDirectory(receiptFolder);

        foreach (var doc in artifacts.ExpenseDocuments)
        {
            var fileName = $"Expenses_{doc.BookIndex}_Page_{doc.PageIndex}.pdf";
            var path = Path.Combine(expenseFolder, fileName);
            await File.WriteAllBytesAsync(path, doc.PdfBytes);
        }

        foreach (var page in artifacts.ReceiptPages)
        {
            var fileName = $"Receipts_Page_{page.PageIndex}.jpeg";
            var path = Path.Combine(receiptFolder, fileName);
            await File.WriteAllBytesAsync(path, page.ImageBytes);
        }
        
        return reportRoot;
    }

    public Task DeleteAsync(Guid reportId)
    {
        var folder = Path.Combine(_wwwroot, "reports", reportId.ToString());
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid reportId)
    {
        var folder = Path.Combine(_wwwroot, "reports", reportId.ToString());
        return Task.FromResult(Directory.Exists(folder));
    }

    public IReadOnlyList<string> GetExpenseFiles(Guid reportId)
    {
        var path = Path.Combine(_wwwroot, "reports", reportId.ToString(), "expenses");

        if (!Directory.Exists(path))
            return [];

        return Directory.GetFiles(path)
            .Order()
            .ToList();
    }

    public IReadOnlyList<string> GetReceiptFiles(Guid reportId)
    {
        var path = Path.Combine(_wwwroot, "reports", reportId.ToString(), "receipts");

        if (!Directory.Exists(path))
            return [];
        
        return Directory
            .GetFiles(path)
            .Order()
            .ToList();
    }
}