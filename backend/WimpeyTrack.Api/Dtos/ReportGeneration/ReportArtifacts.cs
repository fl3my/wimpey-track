namespace WimpeyTrack.Api.Dtos.ReportGeneration;

public class ReportArtifacts
{
    public Guid ReportId { get; set; }
    public IReadOnlyList<ReportFile> Files { get; set; } = [];
}