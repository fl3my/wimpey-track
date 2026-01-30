namespace WimpeyTrack.Api.Dtos.Report;

public class ReportDto
{
    public Guid Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public string FolderPath { get; set; } = string.Empty;
}