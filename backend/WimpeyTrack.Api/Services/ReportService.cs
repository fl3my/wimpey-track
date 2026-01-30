using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Services;

public interface IReportService
{
    Task<Guid?> GenerateAndSaveAsync(DateOnly start, DateOnly end);
    Task<IEnumerable<ReportDto>> GetReportsAsync();
    Task<ReportPreviewDto?> GetReportPreview(Guid reportId);
    Task DeleteAsync(Guid id);
    
}

public class ReportService : IReportService
{
    private readonly IReportGenerationService _generationService;
    private readonly IReportStorageService _storageService;
    private readonly ApplicationDbContext _context;

    public ReportService(IReportGenerationService generationService, IReportStorageService storageService, ApplicationDbContext context)
    {
        _generationService = generationService;
        _storageService = storageService;
        _context = context;
    }

    public async Task<Guid?> GenerateAndSaveAsync(DateOnly start, DateOnly end)
    {
        var artifacts = await _generationService.GenerateAsync(start, end);
        
        if  (artifacts is null)
            return null;
        
        var reportId = Guid.NewGuid();
        var folderPath = await _storageService.SaveAsync(reportId, artifacts);

        _context.Reports.Add(new Report()
        {
            Id = reportId,
            StartDate = start,
            EndDate = end,
            GeneratedAtUtc = DateTime.UtcNow,
            FolderPath = folderPath,
        });
        
        await _context.SaveChangesAsync();
        return reportId;
    }

    public async Task<IEnumerable<ReportDto>> GetReportsAsync()
    {
        return await _context.Reports
            .Select(r => new ReportDto
        {
            Id = r.Id,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            GeneratedAtUtc = r.GeneratedAtUtc,
            FolderPath = r.FolderPath
        }).ToListAsync();
    }

    public async Task<ReportPreviewDto?> GetReportPreview(Guid reportId)
    {
        // If the folder does not exist return
        if (!await _storageService.ExistsAsync(reportId))
            return null;
        
        // Get files from storage
        var expenseFiles =  _storageService.GetExpenseFiles(reportId);
        var receiptFiles = _storageService.GetReceiptFiles(reportId);
        
        // Generate preview
        var preview = new ReportPreviewDto()
        {
            ReportId = reportId,
            ExpenseDocuments = expenseFiles.Select(f => new FileLinkDto
            {
                FileName = Path.GetFileName(f),
                Url = $"/reports/{reportId}/expenses/{Path.GetFileName(f)}"
            }).ToList(),
            ReceiptPages = receiptFiles.Select(f => new FileLinkDto()
            {
                FileName = Path.GetFileName(f),
                Url = $"/reports/{reportId}/receipts/{Path.GetFileName(f)}"
            }).ToList()
        };
        
        return preview;
    }

    public async Task DeleteAsync(Guid reportId)
    {
        var report = await _context.Reports.FindAsync(reportId);

        // If no report, return
        if (report is null)
            return;
        
        // Delete the files 
        await _storageService.DeleteAsync(report.Id);
        
        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
    }
}