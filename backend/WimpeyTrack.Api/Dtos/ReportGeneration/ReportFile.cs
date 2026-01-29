namespace WimpeyTrack.Api.Dtos.ReportGeneration;

public class ReportFile
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = null!;
    public string ContentType = string.Empty;
}