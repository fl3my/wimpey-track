using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.ReportGeneration;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Services;

public interface IReportService
{
    Task<Guid> GenerateAndSaveAsync(DateOnly start, DateOnly end);
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

    public async Task<Guid> GenerateAndSaveAsync(DateOnly start, DateOnly end)
    {
        var artifacts = await _generationService.GenerateAsync(start, end);
        
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
}