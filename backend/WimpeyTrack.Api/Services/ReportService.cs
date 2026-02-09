using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.EmailRecipient;
using WimpeyTrack.Api.Dtos.Report;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Services;

public interface IReportService
{
    Task<Guid?> GenerateAndSaveAsync(DateOnly start, DateOnly end);
    Task<IEnumerable<ReportDto>> GetReportsAsync();
    Task<ReportPreviewDto?> GetReportPreview(Guid reportId);
    Task DeleteAsync(Guid id);
    Task<string?> CreateGmailDraftAsync(Guid reportId, IReadOnlyList<Guid> recipientIds);
}

public class ReportService : IReportService
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
    private readonly ApplicationDbContext _context;
    private readonly IReportGenerationService _generationService;
    private readonly IGmailDraftService _gmailDraftService;
    private readonly IReportStorageService _storageService;

    public ReportService(IReportGenerationService generationService, IReportStorageService storageService,
        ApplicationDbContext context, IGmailDraftService gmailDraftService)
    {
        _generationService = generationService;
        _storageService = storageService;
        _context = context;
        _gmailDraftService = gmailDraftService;
    }

    public async Task<Guid?> GenerateAndSaveAsync(DateOnly start, DateOnly end)
    {
        var artifacts = await _generationService.GenerateAsync(start, end);

        if (artifacts is null)
            return null;

        var reportId = Guid.NewGuid();
        var folderPath = await _storageService.SaveAsync(reportId, artifacts);

        _context.Reports.Add(new Report
        {
            Id = reportId,
            StartDate = start,
            EndDate = end,
            GeneratedAtUtc = DateTime.UtcNow,
            FolderPath = folderPath
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
            }).OrderByDescending(r => r.GeneratedAtUtc).ToListAsync();
    }

    public async Task<ReportPreviewDto?> GetReportPreview(Guid reportId)
    {
        // If the folder does not exist return
        if (!await _storageService.ExistsAsync(reportId))
            return null;

        // Get files from storage
        var expenseFiles = _storageService.GetExpenseFiles(reportId);
        var receiptFiles = _storageService.GetReceiptFiles(reportId);

        // Get Report information
        var report = await _context.Reports.FindAsync(reportId);
        if (report is null)
            return null;

        // Generate preview
        var preview = new ReportPreviewDto
        {
            ReportId = reportId,
            StartDate = report.StartDate,
            EndDate = report.EndDate,
            ExpenseDocuments = expenseFiles.Select(f => new FileLinkDto
            {
                FileName = Path.GetFileName(f),
                Url = $"/reports/{reportId}/expenses/{Path.GetFileName(f)}"
            }).ToList(),
            ReceiptPages = receiptFiles.Select(f => new FileLinkDto
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

    public async Task<string?> CreateGmailDraftAsync(Guid reportId, IReadOnlyList<Guid> recipientIds)
    {
        // Get recipients
        var recipientsData = await _context.EmailRecipients
            .Where(r => recipientIds.Contains(r.Id))
            .ToListAsync();

        // Convert to a tuple
        var recipients = recipientsData.Select(r => (r.FirstName, r.Email))
            .ToList();

        if (recipients.Count != recipientIds.Count)
            throw new InvalidOperationException("One or more recipients are invalid");

        // Get the sender
        var sender = await _context.Profiles.FirstOrDefaultAsync();
        if (sender is null)
            return null;

        var report = await _context.Reports.FindAsync(reportId);
        if (report is null)
            return null;

        // Get files and construct attachments
        var files = _storageService.GetAllFiles(reportId);
        var attachments = files.Select(p =>
        {
            ContentTypeProvider.TryGetContentType(p, out var contentType);

            return new EmailAttachment
            {
                FileName = Path.GetFileName(p),
                Content = File.OpenRead(p),
                ContentType = contentType!
            };
        });

        // Get the greeting
        var recipientNames = recipients.Select(r => r.FirstName).ToList();
        var greeting = recipientNames.Count switch
        {
            1 => $"Hello {recipientNames[0]}",
            2 => $"Hello {recipientNames[0]} and {recipientNames[1]}",
            _ =>
                $"Hello {string.Join(", ", recipientNames.Take(recipientNames.Count - 1))}, and {recipientNames.Last()}"
        };

        var startDate = report.StartDate.ToString("MMMM yyyy");
        var endDate = report.EndDate.ToString("MMMM yyyy");

        // Construct message
        var message = string.Join("\r\n", new[]
        {
            $"{greeting},",
            "",
            $"Here are my expenses from {startDate} to {endDate}.",
            "",
            "Thanks again,",
            sender.FullName
        });
        
        var subject = $"{startDate} – {endDate} Expenses";

        // Create Gmail Draft
        var draftUrl = await _gmailDraftService.CreateDraftAsync(
            sender.FullName,
            recipients,
            subject,
            message,
            attachments);

        return draftUrl;
    }
}