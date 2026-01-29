using WimpeyTrack.Api.Dtos.ReportGeneration;

namespace WimpeyTrack.Api.Services;

public interface IReportStorageService
{
    Task<string> SaveAsync (Guid reportId, ReportArtifacts artifacts);
    Task DeleteAsync(Guid reportId);
    Task<bool> ExistsAsync(Guid reportId);
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
        var folder = Path.Combine(_wwwroot, "reports",  reportId.ToString());
        Directory.CreateDirectory(folder);

        foreach (var file in artifacts.Files)
        {
            var path = Path.Combine(folder, file.FileName);
            await File.WriteAllBytesAsync(path, file.Content);
        }

        return folder;
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
}