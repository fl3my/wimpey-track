namespace WimpeyTrack.Api.Models;

public class Report
{
    public Guid Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public string FolderPath { get; set; } = string.Empty;

}